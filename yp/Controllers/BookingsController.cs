using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using yp.Exceptions;
using yp.Models;

namespace yp.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _service;
        private readonly IEventService _eventService;

        public BookingsController(IBookingService service, IEventService eventService)
        {
            _service = service;
            _eventService = eventService;
        }

        [HttpGet("/bookings/{id}")]
        public async Task<ActionResult<Booking>> Get(Guid id)
        {
            var booking = await _service.GetBookingByIdAsync(id);

            if (booking == null)
                throw new NotFoundException($"Бронь с id {id} не найдена.");

            return Ok(booking);
        }

        [HttpPost("/events/{id}/book")]
        public async Task<ActionResult<Booking>> Book(Guid id)
        {
            var ev = _eventService.Get(id);

            if (ev == null)
                throw new NotFoundException($"Событие с id {id} не найдено.");

            var booking = await _service.CreateBookingAsync(id);

            var location = $"/bookings/{booking.Id}";

            return Accepted(location, booking);
        }
    }
}
