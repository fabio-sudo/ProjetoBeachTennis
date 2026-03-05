using ArenaBackend.Data;
using ArenaBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly ArenaDbContext _context;
        public SuppliersController(ArenaDbContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetAll()
            => await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Supplier>> GetById(int id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            return s == null ? NotFound() : Ok(s);
        }

        [HttpPost]
        public async Task<ActionResult<Supplier>> Create(Supplier supplier)
        {
            supplier.CreatedAt = DateTime.Now;
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Supplier supplier)
        {
            if (id != supplier.Id) return BadRequest();
            _context.Entry(supplier).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.Suppliers.Any(s => s.Id == id)) return NotFound(); throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return NotFound();
            _context.Suppliers.Remove(s);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
