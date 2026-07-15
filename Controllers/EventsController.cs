using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using yp.Models;
using System.Linq;

namespace yp.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _service;

        public EventsController(IEventService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<EventDTO>> GetAll()
        {
            var items = _service.GetAll();
            var dtos = items.Select(ToDto);

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public ActionResult<EventDTO> Get(Guid id)
        {
            var ev = _service.Get(id);

            if (ev == null) return NotFound();

            return Ok(ToDto(ev));
        }

        [HttpPost]
        public ActionResult<EventDTO> Create([FromBody] EventDTO ev)
        {
            if (ev == null) return BadRequest();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var model = ToModel(ev);
            _service.Create(model);
            var createdDto = ToDto(model);

            return CreatedAtAction(nameof(Get), new { id = createdDto.Id }, createdDto);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] EventDTO ev)
        {
            if (ev == null) return BadRequest();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var model = ToModel(ev);
            var updated = _service.Update(id, model);

            if (!updated) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var deleted = _service.Delete(id);

            if (!deleted) return NotFound();

            return NoContent();
        }

        private static EventDTO ToDto(Event e)
        {
            return new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartAt = e.StartAt,
                EndAt = e.EndAt
            };
        }

        private static Event ToModel(EventDTO dto)
        {
            return new Event
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                StartAt = dto.StartAt,
                EndAt = dto.EndAt
            };
        }
    }
}
