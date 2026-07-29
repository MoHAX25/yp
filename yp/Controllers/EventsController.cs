using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using yp.Exceptions;
using yp.Models;

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
        public ActionResult<PaginatedResult<EventDTO>> GetAll(
            [FromQuery] string? title,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (from.HasValue && to.HasValue && from.Value > to.Value)
                throw new ValidationAppException("Параметр from не может быть позже to.");

            if (page < 1)
                throw new ValidationAppException("Параметр page должен быть больше или равен 1.");

            if (pageSize < 1)
                throw new ValidationAppException("Параметр pageSize должен быть больше или равен 1.");

            if (pageSize > 100) 
                throw new ValidationAppException("Параметр pageSize не может быть больше 100.");

            var result = _service.GetAll(title, from, to, page, pageSize);

            var dto = new PaginatedResult<EventDTO>
            {
                Items = result.Items.Select(ToDto),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return Ok(dto);
        }

        [HttpGet("{id}")]
        public ActionResult<EventDTO> Get(Guid id)
        {
            var ev = _service.Get(id);

            if (ev == null)
                throw new NotFoundException($"Событие с id {id} не найдено.");

            return Ok(ToDto(ev));
        }

        [HttpPost]
        public ActionResult<EventDTO> Create([FromBody] EventDTO ev)
        {
            if (ev == null) throw new ValidationAppException("Тело запроса не может быть пустым.");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    );
                throw new ValidationAppException("Некорректные данные запроса.", errors);
            }

            var model = ToModel(ev);
            var created = _service.Create(model);

            ev.Id = created.Id;

            return CreatedAtAction(nameof(Get), new { id = ev.Id }, ev);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] EventDTO ev)
        {
            if (ev == null) throw new ValidationAppException("Тело запроса не может быть пустым.");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    );
                throw new ValidationAppException("Некорректные данные запроса.", errors);
            }
                

            var model = ToModel(ev);
            var updated = _service.Update(id, model);

            if (!updated) throw new NotFoundException($"Событие с id {id} не найдено.");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var deleted = _service.Delete(id);

            if (!deleted) throw new NotFoundException($"Событие с id {id} не найдено.");

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
