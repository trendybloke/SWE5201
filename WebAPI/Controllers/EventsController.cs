#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly WebAPIContext _context;

        public EventsController(WebAPIContext context)
        {
            _context = context;
        }

        #region GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvent()
        {
            return await _context.Event.ToListAsync();
        }
        #endregion

        #region GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var @event = await _context.Event.FindAsync(id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }
        #endregion

        #region GET: api/Events/WithTags
        [HttpGet("WithTags/{title?}")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEventsWithTags
            (string title = null)
        {
            var queryableEvents =
                this._context
                        .Event
                        .Include(x => x.Tags)
                        .AsNoTracking()
                        .AsQueryable();

            if(!string.IsNullOrEmpty(title))
                queryableEvents = queryableEvents.Where(x => x.Title.Contains(title));

            var results = await queryableEvents.ToListAsync();

            return results;
        }


        #endregion

        #region PUT: api/Events/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, Event @event)
        {
            if (id != @event.Id)
            {
                return BadRequest();
            }

            var existingEvent = 
                await _context.Event
                                .Include(x => x.Tags)
                                .Where(x => x.Id == id)
                                .SingleOrDefaultAsync();

            if(existingEvent != null)
            {
                // Update Event
                _context.Entry(existingEvent)
                    .CurrentValues
                    .SetValues(@event);

                // Remove now non existing tags in Event
                foreach(var existingTag in existingEvent.Tags.ToList())
                {
                    if (!existingEvent.Tags.Any(t => t.Id == existingTag.Id))
                        existingEvent.Tags.Remove(existingTag);
                }

                // Add new tags
                foreach(var newTag in @event.Tags)
                {
                    if(!existingEvent.Tags.Any(t => t.Id == newTag.Id))
                        existingEvent.Tags.Add(newTag);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
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
        #endregion

        #region POST: api/Events
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event @event)
        {
            try
            {
                foreach(var tag in @event.Tags)
                    _context.Tag.Attach(tag);



                _context.Event.Add(@event);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return CreatedAtAction("GetEvent", new { id = @event.Id }, @event);
        }
        #endregion

        #region DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var @event = await _context.Event.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            _context.Event.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.Id == id);
        }
    }
}
