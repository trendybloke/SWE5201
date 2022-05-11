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
    public class HostedEventsController : ControllerBase
    {
        private readonly WebAPIContext _context;

        public HostedEventsController(WebAPIContext context)
        {
            _context = context;
        }

        # region GET: api/HostedEvents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HostedEvent>> GetHostedEvent(int id)
        {
            var hostedEvent = await _context.HostedEvent.FindAsync(id);

            if (hostedEvent == null)
            {
                return NotFound();
            }

            return hostedEvent;
        }
        #endregion

        #region GET: api/HostedEvents
        // https://localhost:7269/api/HostedEvents
        [HttpGet]
        public async Task<IEnumerable<HostedEvent>> 
            GetHostedEventsWithEventAndTags
                (string title = null, string tagsStr = null)
        {
            var queryableHostedEvents =
                this._context
                        .HostedEvent
                        .Include(x => x.Event)
                        .Include(x => x.Room)                        
                        .AsNoTracking()
                        .AsQueryable();

            var results = await queryableHostedEvents.ToListAsync();

            foreach(var hostedEvent in results)
            {
                var result = await _context.Event
                                                .Include(x => x.Tags)
                                                .AsNoTracking()
                                                .AsQueryable()
                                                .Where(x => x.Id == hostedEvent._EventId)
                                                .FirstOrDefaultAsync();
                hostedEvent.Event = result;
            }

            IEnumerable<HostedEvent> queriedResults = results;

            if (!string.IsNullOrEmpty(title))
            {
                queriedResults = results.Where(x => x.Event.Title.Contains(title));
            }

            if (!string.IsNullOrEmpty(tagsStr))
            {
                string[] tags;

                if (tagsStr.Contains(", "))
                    tags = tagsStr.Split(", ");
                else
                    tags = new string[] { tagsStr };

                foreach (string tag in tags)
                {
                    queriedResults =
                        queriedResults
                        .Where(x => x.Event.Tags.Any(t => t.Content.Contains(tag)));
                }
            }

            return queriedResults;
        }
        #endregion

        #region GET: api/HostedEvents/Upcoming
        [HttpGet("Upcoming")]
        public async Task<IEnumerable<HostedEvent>> GetUpcomingHostedEvents
            (string title = null, string tagsStr = null)
        {
            var queryableHostedEvents = 
                this._context
                        .HostedEvent
                        .Include(x => x.Event)
                        .Include(x => x.Room)
                        .AsNoTracking()
                        .AsQueryable();

            var results = await queryableHostedEvents.ToListAsync();

            foreach(var hostedEvent in results)
            {
                var result = await _context.Event
                                            .Include(x => x.Tags)
                                            .AsNoTracking()
                                            .AsQueryable()
                                            .Where(x => x.Id == hostedEvent._EventId)
                                            .FirstOrDefaultAsync();

                hostedEvent.Event = result;
            }

            var upcomingResults = results.Where(x => x.EndTime > DateTime.Now);

            if (!string.IsNullOrEmpty(title))
            {
                upcomingResults = 
                    upcomingResults.Where(x => x.Event.Title.Contains(title));
            }

            if (!string.IsNullOrEmpty(tagsStr))
            {
                string[] tags;

                if (tagsStr.Contains(", "))
                    tags = tagsStr.Split(", ");
                else
                    tags = new string[] { tagsStr };

                foreach(string tag in tags)
                {
                    upcomingResults =
                        upcomingResults
                        .Where(x => x.Event.Tags.Any(t => t.Content.Contains(tag)));
                }
            }

            return upcomingResults;
        }
        #endregion

        #region PUT: api/HostedEvents/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHostedEvent(int id, HostedEvent hostedEvent)
        {
            if (id != hostedEvent.Id)
            {
                return BadRequest();
            }

            _context.Entry(hostedEvent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HostedEventExists(id))
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

        #region POST: api/HostedEvents
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<HostedEvent>> PostHostedEvent(HostedEvent hostedEvent)
        {
            try 
            {
                if(hostedEvent.Event != null)
                    _context.Event.Attach(hostedEvent.Event);

                if(hostedEvent.Room != null)
                    _context.Room.Attach(hostedEvent.Room);
                
                _context.HostedEvent.Add(hostedEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return CreatedAtAction("GetHostedEvent", new { id = hostedEvent.Id }, hostedEvent);
        }
        #endregion

        # region DELETE: api/HostedEvents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHostedEvent(int id)
        {
            var hostedEvent = await _context.HostedEvent.FindAsync(id);
            if (hostedEvent == null)
            {
                return NotFound();
            }

            _context.HostedEvent.Remove(hostedEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        private bool HostedEventExists(int id)
        {
            return _context.HostedEvent.Any(e => e.Id == id);
        }
    }
}
