using ArenaBackend.Data;
using ArenaBackend.Models;
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
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations
                .Include(r => r.Court)
                .ToListAsync();
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

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
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
