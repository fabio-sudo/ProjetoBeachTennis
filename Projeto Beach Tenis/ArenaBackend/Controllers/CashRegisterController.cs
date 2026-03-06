using ArenaBackend.Data;
using ArenaBackend.DTOs;
using ArenaBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CashRegisterController : ControllerBase
    {
        private readonly ArenaDbContext _context;
        public CashRegisterController(ArenaDbContext context) { _context = context; }

        // GET: api/cashregister/current
        [HttpGet("current")]
        public async Task<ActionResult<object>> GetCurrent()
        {
            var register = await _context.CashRegisters
                .Include(cr => cr.Transactions)
                .FirstOrDefaultAsync(cr => cr.Status == "Open");

            if (register == null) return Ok(null);

            return Ok(BuildSummary(register));
        }

        // GET: api/cashregister
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var registers = await _context.CashRegisters
                .Include(cr => cr.Transactions)
                .OrderByDescending(cr => cr.OpenedAt)
                .ToListAsync();

            return Ok(registers.Select(r => BuildSummary(r)));
        }

        // GET: api/cashregister/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var register = await _context.CashRegisters
                .Include(cr => cr.Transactions).ThenInclude(t => t.Sale)
                .FirstOrDefaultAsync(cr => cr.Id == id);

            if (register == null) return NotFound();

            return Ok(BuildSummary(register));
        }

        // POST: api/cashregister/open
        [HttpPost("open")]
        public async Task<ActionResult> Open(OpenCashRegisterDto dto)
        {
            var existing = await _context.CashRegisters.AnyAsync(cr => cr.Status == "Open");
            if (existing) return BadRequest("Já existe um caixa aberto. Feche-o antes de abrir um novo.");

            var register = new CashRegister
            {
                OpeningAmount = dto.OpeningAmount,
                OpenedAt = DateTime.Now,
                Status = "Open"
            };
            _context.CashRegisters.Add(register);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Caixa aberto com sucesso.", id = register.Id });
        }

        // POST: api/cashregister/close
        [HttpPost("close")]
        public async Task<ActionResult> Close(CloseCashRegisterDto dto)
        {
            var register = await _context.CashRegisters
                .Include(cr => cr.Transactions)
                .FirstOrDefaultAsync(cr => cr.Status == "Open");

            if (register == null) return NotFound("Nenhum caixa aberto encontrado.");

            register.ClosingAmount = dto.ClosingAmount;
            register.ClosedAt = DateTime.Now;
            register.Status = "Closed";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Caixa fechado com sucesso.", summary = BuildSummary(register) });
        }

        // POST: api/cashregister/transactions
        [HttpPost("transactions")]
        public async Task<ActionResult> AddTransaction(AddCashTransactionDto dto)
        {
            var register = await _context.CashRegisters.FirstOrDefaultAsync(cr => cr.Status == "Open");
            if (register == null) return NotFound("Nenhum caixa aberto.");

            var tx = new CashTransaction
            {
                CashRegisterId = register.Id,
                Type = dto.Type,
                Amount = dto.Amount,
                Description = dto.Description,
                CreatedAt = DateTime.Now
            };
            _context.CashTransactions.Add(tx);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Transação registrada.", id = tx.Id });
        }

        private static object BuildSummary(CashRegister r)
        {
            var salesTotal = r.Transactions.Where(t => t.Type == "Sale").Sum(t => t.Amount);
            var expensesTotal = r.Transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            var adjustmentsTotal = r.Transactions.Where(t => t.Type == "Adjustment").Sum(t => t.Amount);
            var balance = r.OpeningAmount + salesTotal - expensesTotal + adjustmentsTotal;

            return new
            {
                r.Id,
                r.Status,
                r.OpenedAt,
                r.ClosedAt,
                r.OpeningAmount,
                r.ClosingAmount,
                SalesTotal = salesTotal,
                ExpensesTotal = expensesTotal,
                AdjustmentsTotal = adjustmentsTotal,
                ExpectedBalance = balance,
                Transactions = r.Transactions.OrderByDescending(t => t.CreatedAt).Select(t => new
                {
                    t.Id,
                    t.Type,
                    t.Amount,
                    t.Description,
                    t.SaleId,
                    t.CreatedAt
                })
            };
        }

        // --- New Requested Endpoints ---

        [HttpGet("/api/cash/history")]
        public async Task<ActionResult<IEnumerable<object>>> GetCashHistory()
        {
            var registers = await _context.CashRegisters
                .Where(cr => cr.Status == "Closed" || cr.Status == "Cancelled")
                .Include(cr => cr.Transactions)
                .OrderByDescending(cr => cr.OpenedAt)
                .ToListAsync();

            return Ok(registers.Select(r => BuildSummary(r)));
        }

        [HttpGet("/api/cash/{id}")]
        public async Task<ActionResult<object>> GetCashByIdPath(int id)
        {
            var register = await _context.CashRegisters
                .Include(cr => cr.Transactions).ThenInclude(t => t.Sale)
                .FirstOrDefaultAsync(cr => cr.Id == id);

            if (register == null) return NotFound();

            return Ok(BuildSummary(register));
        }

        [HttpPost("/api/cash/{id}/cancel")]
        public async Task<IActionResult> CancelCashRegister(int id)
        {
            var register = await _context.CashRegisters.FindAsync(id);
            if (register == null) return NotFound();
            if (register.Status == "Cancelled") return BadRequest("Caixa já está cancelado.");

            register.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Caixa cancelado com sucesso." });
        }

        [HttpPut("/api/cash/{id}")]
        public async Task<IActionResult> EditCashRegister(int id, [FromBody] EditCashDto dto)
        {
            var register = await _context.CashRegisters
                .Include(cr => cr.Transactions)
                .FirstOrDefaultAsync(cr => cr.Id == id);

            if (register == null) return NotFound();

            if (dto.OpeningAmount.HasValue) register.OpeningAmount = dto.OpeningAmount.Value;
            if (dto.ClosingAmount.HasValue) register.ClosingAmount = dto.ClosingAmount.Value;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Valores atualizados com sucesso.", summary = BuildSummary(register) });
        }
    }

    public class EditCashDto
    {
        public decimal? OpeningAmount { get; set; }
        public decimal? ClosingAmount { get; set; }
    }
}
