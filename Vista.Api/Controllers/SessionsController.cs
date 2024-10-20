using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vista.Api.Data;
using Vista.Api.Dtos;

namespace Vista.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly TrainersDbContext _context;

        public SessionsController(TrainersDbContext context)
        {
            _context = context;
        }

        // GET: api/Sessions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Session>>> GetSessions()
        {
            return await _context.Sessions.ToListAsync();
        }

        // GET: api/Sessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Session>> GetSession(int id)
        {
            var session = await _context.Sessions.FindAsync(id);

            if (session == null)
            {
                return NotFound();
            }

            return session;
        }

        // GET: api/ GetFreeSessions?date=yyyy-mm-dd&category=aa
        // Gets a list of sessions that are not booked for a specified date and category
        [HttpGet("GetFreeSessions")]
        public async Task<ActionResult<IEnumerable<SessionFreeSlotDto>>> GetFreeSessions(DateTime date, string category)
        {
            var sessions = await _context.Sessions
                .Where(s => s.SessionDate == date
                    && s.BookingReference == null
                    && s.Trainer!.TrainerCategories != null
                    && s.Trainer.TrainerCategories
                        .Any(tr => tr.Category!.CategoryCode == category))
                .Select(s => new SessionFreeSlotDto
                {
                    SessionId = s.SessionId,
                    SessionDate = s.SessionDate,
                    TrainerId = s.TrainerId,
                    TrainerName = s.Trainer!.Name
                }).ToListAsync();

            return sessions;
        }

        // PUT: api/BookSession/5
        // Book session
        [HttpPut("BookSession/{id}")]
        public async Task<IActionResult> BookSession(int id, SessionBookingRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != request.SessionId) {
                return BadRequest(ModelState);
            }

            var session = await _context.Sessions
                .Include(s => s.Trainer) // Include this to get the trainer name
                .Where(s => s.SessionId == id)
                .FirstOrDefaultAsync();

            //Check if already booked?
            if(session.BookingReference != null)
            {
                return BadRequest("Session already booked");
            }

            var bookRef = Guid.NewGuid().ToString(); //Universally Unique Identifier (UUID)
            session.BookingReference = bookRef;
            _context.Entry(session).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
                //Log error
                //return StatusCode(500);
            }

            //Create a return the booking details using a DTO
            return Ok(new SessionBookingDto
            {
                SessionId = session.SessionId,
                SessionDate = session.SessionDate,
                TrainerId = session.TrainerId,
                TrainerName = session.Trainer!.Name,
                BookingReference = bookRef
            });
        }

        // POST: api/Sessions/AddSessionDate
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("AddSessionDate")]
        public async Task<ActionResult<Session>> AddSessionDate(Session session)
        {
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSession", new { id = session.SessionId }, session);
        }

        // DELETE: api/Sessions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SessionExists(int id)
        {
            return _context.Sessions.Any(e => e.SessionId == id);
        }
    }
}
