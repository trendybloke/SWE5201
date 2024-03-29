﻿#nullable disable
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
    public class TagsController : ControllerBase
    {
        private readonly WebAPIContext _context;

        public TagsController(WebAPIContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTag
            (string content = null)
        {
            var queryableTags =
                this._context
                        .Tag
                        .Include(x => x.Events)
                        .AsNoTracking()
                        .AsQueryable();

            if (!string.IsNullOrEmpty(content))
                queryableTags
                    = queryableTags.Where(x => x.Content.Contains(content));

            var results = await queryableTags.ToListAsync();

            return results;
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTag(int id)
        {
            var tag = await _context.Tag.FindAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            return tag;
        }

        // PUT: api/Tags/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTag(int id, Tag tag)
        {
            if (id != tag.Id)
            {
                return BadRequest();
            }

            var existingTag =
                await _context.Tag
                            .Include(x => x.Events)
                            .Where(x => x.Id == id)
                            .SingleOrDefaultAsync();

            if(existingTag != null)
            {
                // Update Tag
                _context.Entry(existingTag)
                    .CurrentValues
                    .SetValues(tag);

                // Remove non existing Events in Tag
                foreach(var existingEvent in existingTag.Events.ToList())
                {
                    if(!tag.Events.Any(e => e.Id == existingEvent.Id))
                        existingTag.Events.Remove(existingEvent);
                }

                // Add new Events
                foreach(var newEvent in tag.Events)
                {
                    if (!existingTag.Events.Any(e => e.Id == newEvent.Id))
                        existingTag.Events.Add(newEvent);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
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

        // POST: api/Tags
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tag>> PostTag(Tag tag)
        {
            _context.Tag.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTag", new { id = tag.Id }, tag);
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _context.Tag.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            _context.Tag.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TagExists(int id)
        {
            return _context.Tag.Any(e => e.Id == id);
        }
    }
}
