using ArenaBackend.Data;
using ArenaBackend.Models;
using ArenaBackend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly ArenaDbContext _context;

        public ReservationsController(ArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int? courtId = null)
        {
            var query = _context.Reservations.Include(r => r.Court).AsQueryable();

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(r => r.ReservationDate >= start.Date);
            }
            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            {
                query = query.Where(r => r.ReservationDate <= end.Date);
            }
            if (courtId.HasValue)
            {
                query = query.Where(r => r.CourtId == courtId.Value);
            }

            var results = await query.ToListAsync();
            return Ok(results);
        }

        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Court)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        // PUT: api/Reservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(int id, Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return BadRequest();
            }

            // Conflict Validation for Update
            if (await HasConflict(reservation.CourtId, reservation.ReservationDate, reservation.StartTime, reservation.EndTime, id))
            {
                return BadRequest("Court is already booked for this time period.");
            }

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Reservations
        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation(Reservation reservation)
        {
            // Conflict Validation
            if (await HasConflict(reservation.CourtId, reservation.ReservationDate, reservation.StartTime, reservation.EndTime))
            {
                return BadRequest("Court is already booked for this time period.");
            }

            reservation.CreatedAt = DateTime.Now;
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // Refund if paid
            if (reservation.Status == "Finalizado")
            {
                var openRegister = await _context.CashRegisters.FirstOrDefaultAsync(cr => cr.Status == "Open");
                if (openRegister != null)
                {
                    _context.CashTransactions.Add(new CashTransaction
                    {
                        CashRegisterId = openRegister.Id,
                        Type = "Adjustment",
                        Amount = -reservation.Price,
                        Description = $"Cancelamento: Agendamento Pago #{reservation.Id} - {reservation.CustomerName}",
                        CreatedAt = DateTime.Now
                    });
                }
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

        // POST: api/Reservations/{id}/checkout
        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> CheckoutReservation(int id, [FromBody] CheckoutDto dto)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound("Reserva não encontrada.");

            if (reservation.Status == "Finalizado") return BadRequest("Esta reserva já foi finalizada/paga.");

            // Update Reservation Status
            reservation.Status = "Finalizado";
            reservation.PaymentType = dto.PaymentType;

            // Register in CashFlow if Open
            var openRegister = await _context.CashRegisters.FirstOrDefaultAsync(cr => cr.Status == "Open");
            if (openRegister != null)
            {
                _context.CashTransactions.Add(new CashTransaction
                {
                    CashRegisterId = openRegister.Id,
                    Type = "Service",
                    Amount = reservation.Price,
                    Description = $"Agendamento #{reservation.Id} - {reservation.CustomerName} ({dto.PaymentType})",
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Pagamento registrado com sucesso", reservation });
        }

        private async Task<bool> HasConflict(int courtId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var query = _context.Reservations
                .Where(r => r.CourtId == courtId && r.ReservationDate.Date == date.Date);

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.Id != excludeId.Value);
            }

            var conflictingReservations = await query.ToListAsync();

            foreach (var r in conflictingReservations)
            {
                // Conflict occurs if:
                // Existing booking starts BEFORE the new booking ends
                // AND
                // Existing booking ends AFTER the new booking starts
                if (r.StartTime < endTime && r.EndTime > startTime)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
