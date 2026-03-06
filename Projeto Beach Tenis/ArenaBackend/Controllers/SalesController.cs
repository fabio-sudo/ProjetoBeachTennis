using ArenaBackend.Data;
using ArenaBackend.Models;
using ArenaBackend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly ArenaDbContext _context;

        public SalesController(ArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Sales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sale>>> GetSales()
        {
            return await _context.Sales
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .ToListAsync();
        }

        // GET: api/Sales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Sale>> GetSale(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            return sale;
        }

        // POST: api/Sales
        [HttpPost]
        public async Task<ActionResult<Sale>> PostSale(Sale sale)
        {
            sale.SaleDate = DateTime.Now;
            decimal calculatedTotal = 0;

            if (sale.Items == null || !sale.Items.Any())
            {
                return BadRequest("A venda deve conter pelo menos um item.");
            }

            foreach (var item in sale.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    return BadRequest($"Produto com ID {item.ProductId} não encontrado.");
                }

                if (product.Stock < item.Quantity)
                {
                    return BadRequest($"Estoque insuficiente para o produto: {product.Name}. Disponível: {product.Stock}");
                }

                // Deduct stock
                product.Stock -= item.Quantity;

                // Ensure unit price is correct (from DB)
                item.UnitPrice = product.Price;
                calculatedTotal += item.UnitPrice * item.Quantity;
            }

            sale.TotalAmount = calculatedTotal;

            // Handle default customer "Consumidor" if none selected
            if (!sale.CustomerId.HasValue && !sale.StudentId.HasValue)
            {
                var defaultCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Name == "Consumidor");
                if (defaultCustomer == null)
                {
                    defaultCustomer = new Customer { Name = "Consumidor", Phone = "-" };
                    _context.Customers.Add(defaultCustomer);
                    await _context.SaveChangesAsync();
                }
                sale.CustomerId = defaultCustomer.Id;
            }

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            // Register in open cash register if one exists
            var openRegister = await _context.CashRegisters.FirstOrDefaultAsync(cr => cr.Status == "Open");
            if (openRegister != null)
            {
                _context.CashTransactions.Add(new CashTransaction
                {
                    CashRegisterId = openRegister.Id,
                    Type = "Sale",
                    Amount = sale.TotalAmount,
                    Description = $"Venda #{sale.Id} - {sale.PaymentType}",
                    SaleId = sale.Id,
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
        }

        // DELETE: api/Sales/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            // Restore stock
            foreach (var item in sale.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                }
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: /api/orders/{id}/cancel
        [HttpPost("/api/orders/{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound("Venda/Pedido não encontrado.");
            if (sale.Status == "Cancelled") return BadRequest("Este pedido já está cancelado.");

            // Restore stock
            foreach (var item in sale.Items)
            {
                if (item.Product != null)
                {
                    item.Product.Stock += item.Quantity;
                }
            }

            sale.Status = "Cancelled";

            // If the register is open, log the reversal
            var openRegister = await _context.CashRegisters.FirstOrDefaultAsync(cr => cr.Status == "Open");
            if (openRegister != null)
            {
                _context.CashTransactions.Add(new CashTransaction
                {
                    CashRegisterId = openRegister.Id,
                    Type = "Adjustment",
                    Amount = -sale.TotalAmount,
                    Description = $"Cancelamento: Venda #{sale.Id}",
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Pedido cancelado com sucesso. Estoque restaurado e valor estornado no caixa." });
        }

        // POST: api/Sales/{id}/items/{itemId}/cancel
        [HttpPost("{id}/items/{itemId}/cancel")]
        public async Task<IActionResult> CancelSaleItem(int id, int itemId, [FromBody] CancelActionDto dto)
        {
            var sale = await _context.Sales
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound("Venda não encontrada.");

            var item = sale.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return NotFound("Item não encontrado nesta venda.");

            // Restore stock
            if (item.Product != null)
            {
                item.Product.Stock += item.Quantity;
            }

            var amountToRefund = item.UnitPrice * item.Quantity;
            sale.TotalAmount -= amountToRefund;

            var openRegister = await _context.CashRegisters.FirstOrDefaultAsync(cr => cr.Status == "Open");
            if (openRegister != null)
            {
                _context.CashTransactions.Add(new CashTransaction
                {
                    CashRegisterId = openRegister.Id,
                    Type = "Adjustment",
                    Amount = -amountToRefund,
                    Description = $"Cancelamento: Venda #{sale.Id} Item '{item.Product?.Name}' - Motivo: {dto.Reason} ({dto.CancelledBy})",
                    CreatedAt = DateTime.Now
                });
            }

            _context.Remove(item);

            // If this was the last item, delete the entire sale
            if (sale.Items.Count <= 1)
            {
                // First check if a Tab references this Sale and unlink/cancel it to prevent FK constraint error
                var linkedTab = await _context.Tabs.FirstOrDefaultAsync(t => t.SaleId == sale.Id);
                if (linkedTab != null)
                {
                    linkedTab.SaleId = null;
                    linkedTab.Status = "Cancelled";
                }

                // Unlink CashTransactions to prevent FK constraint error
                var linkedTransactions = await _context.CashTransactions.Where(ct => ct.SaleId == sale.Id).ToListAsync();
                foreach (var tx in linkedTransactions)
                {
                    tx.SaleId = null;
                }

                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Último item cancelado. Venda inteira foi removida.", refund = amountToRefund, newTotal = 0, saleDeleted = true });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Item cancelado com sucesso", refund = amountToRefund, newTotal = sale.TotalAmount, saleDeleted = false });
        }

        private bool SaleExists(int id)
        {
            return _context.Sales.Any(e => e.Id == id);
        }
    }
}
