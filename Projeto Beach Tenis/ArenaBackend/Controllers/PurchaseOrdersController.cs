using ArenaBackend.Data;
using ArenaBackend.DTOs;
using ArenaBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly ArenaDbContext _context;
        public PurchaseOrdersController(ArenaDbContext context) { _context = context; }

        // GET: api/purchaseorders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var orders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Items).ThenInclude(i => i.Product)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();

            return Ok(orders.Select(po => new
            {
                po.Id,
                po.OrderDate,
                po.Status,
                po.Notes,
                SupplierName = po.Supplier?.Name,
                TotalCost = po.Items.Sum(i => i.Quantity * i.CostPrice),
                ItemCount = po.Items.Count,
                Items = po.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product?.Name,
                    i.Quantity,
                    i.CostPrice,
                    Subtotal = i.Quantity * i.CostPrice
                })
            }));
        }

        // GET: api/purchaseorders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (po == null) return NotFound();

            return Ok(new
            {
                po.Id,
                po.OrderDate,
                po.Status,
                po.Notes,
                po.SupplierId,
                SupplierName = po.Supplier?.Name,
                TotalCost = po.Items.Sum(i => i.Quantity * i.CostPrice),
                Items = po.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product?.Name,
                    i.Quantity,
                    i.CostPrice,
                    Subtotal = i.Quantity * i.CostPrice
                })
            });
        }

        // POST: api/purchaseorders
        [HttpPost]
        public async Task<ActionResult> Create(CreatePurchaseOrderDto dto)
        {
            if (!dto.Items.Any()) return BadRequest("O pedido deve ter pelo menos um item.");

            var supplier = await _context.Suppliers.FindAsync(dto.SupplierId);
            if (supplier == null) return BadRequest("Fornecedor não encontrado.");

            var order = new PurchaseOrder
            {
                SupplierId = dto.SupplierId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Notes = dto.Notes,
                Items = dto.Items.Select(i => new PurchaseOrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CostPrice = i.CostPrice
                }).ToList()
            };

            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, new { order.Id, order.Status });
        }

        // POST: api/purchaseorders/{id}/receive
        [HttpPost("{id}/receive")]
        public async Task<ActionResult> Receive(int id)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null) return NotFound();
            if (order.Status != "Pending") return BadRequest($"Este pedido não pode ser recebido. Status atual: {order.Status}");

            // Update stock for each item and update product cost price
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    product.CostPrice = item.CostPrice; // update cost price
                }
            }

            order.Status = "Received";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Pedido recebido. Estoque atualizado." });
        }

        // POST: api/purchaseorders/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> Cancel(int id)
        {
            var order = await _context.PurchaseOrders.FindAsync(id);
            if (order == null) return NotFound();
            if (order.Status == "Received") return BadRequest("Pedidos já recebidos não podem ser cancelados.");
            if (order.Status == "Cancelled") return BadRequest("Este pedido já está cancelado.");

            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Pedido cancelado." });
        }
    }
}
