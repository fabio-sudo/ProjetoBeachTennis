using ArenaBackend.Data;
using ArenaBackend.Models;
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
                if (defaultCustomer != null)
                {
                    sale.CustomerId = defaultCustomer.Id;
                }
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

        private bool SaleExists(int id)
        {
            return _context.Sales.Any(e => e.Id == id);
        }
    }
}
