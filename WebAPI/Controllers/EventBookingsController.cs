using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventBookingsController : ControllerBase
    {
        private readonly WebAPIContext _context;

        public EventBookingsController(WebAPIContext context)
        {
            _context = context;
        }

        #region GET: api/EventBookings/{BookingId}
        [HttpGet("{BookingId}")]
        public async Task<ActionResult<EventBooking>> GetEventBooking
            (int BookingId)
        {
            var booking = await _context.EventBooking.FindAsync(BookingId);

            if (booking == null)
                return NotFound();

            return booking;
        }

        #endregion

        #region GET: api/EventBookings/ByUser/{UserId}
        [HttpGet("ByUser/{UserId}")]
        public async Task<IEnumerable<EventBooking>> GetUsersEventBookings
            (string UserId)
        {
            var allBookings = this._context.EventBooking
                                                //.Include(x => x.User)
                                                .Include(x => x.HostedEvent)
                                                .AsNoTracking()
                                                .AsQueryable();

            var results = await allBookings.Where(x => x.UserId == UserId).ToListAsync();

            return results;
        }
        #endregion

        #region GET: api/EventBookings/{HostedEventId}
        [HttpGet("ByHostedEvent/{HostedEventId}")]
        public async Task<IEnumerable<EventBooking>> GetUsersBookedToEvent
            (int HostedEventId)
        {
            var allBookings = this._context.EventBooking
                                                //.Include(x => x.User)
                                                .Include(x => x.HostedEvent)
                                                .AsNoTracking()
                                                .AsQueryable();

            try
            {
                var results = await allBookings.Where(x => x.HostedEventId == HostedEventId).ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                return new List<EventBooking>();
            }
        }
        #endregion

        #region POST: api/EventBookings/Book
        [HttpPost("Book")]
        public async Task<ActionResult<EventBooking>> BookEvent
            ([FromBody] EventBooking eventBooking)
        {
            try
            {
                if(eventBooking.HostedEvent != null)
                    _context.HostedEvent.Attach(eventBooking.HostedEvent);

                //if(eventBooking.User != null)
                //    _context.Users.Attach(eventBooking.User);

                _context.EventBooking.Add(eventBooking);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return CreatedAtAction("GetEventBooking", new { BookingId = eventBooking.Id }, eventBooking);
        }
        #endregion

        #region PUT api/EventBookings/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBookedEvent(int id, EventBooking eventBooking)
        {
            if (id != eventBooking.Id)
                return BadRequest();

            var existingBooking =
                await _context.EventBooking
                                .Include(x => x.HostedEvent)
                                //.Include(x => x.User)
                                .Where(x => x.Id == id)
                                .SingleOrDefaultAsync();

            if(existingBooking != null)
            {
                // Update booking
                _context.Entry(existingBooking)
                    .CurrentValues
                    .SetValues(eventBooking);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                if (!_context.EventBooking.Any(b => b.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }
        #endregion

        #region DELETE api/EventBookings/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookedEvent(int id)
        {
            var bookedEvent = await _context.EventBooking.FindAsync(id);

            if (bookedEvent == null)
                return NotFound();

            _context.EventBooking.Remove(bookedEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion
    }
}
