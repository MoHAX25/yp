using Microsoft.AspNetCore.Mvc;
using yp.Services;
using yp.Exceptions;
using yp.Models;

namespace yp.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingsController(IBookingService service)
        {
            _service = service;
        }

        [HttpGet("/bookings/{id}")]
        public async Task<ActionResult<Booking>> Get(Guid id)
        {
            var booking = await _service.GetBookingByIdAsync(id);

            if (booking == null)
                throw new NotFoundException($"Бронь с id {id} не найдена.");

            return Ok(ToDto(booking));
        }

        [HttpPost("/events/{id}/book")]
        public async Task<ActionResult<Booking>> Book(Guid id)
        {
            var booking = await _service.CreateBookingAsync(id);

            var location = $"/bookings/{booking.Id}";

            return Accepted(location, ToDto(booking));
        }

        private static BookingDTO ToDto(Booking e)
        {
            return new BookingDTO
            {
                Id = e.Id,
                EventId = e.EventId,
                Status = e.Status,
                CreatedAt = e.CreatedAt,
                ProcessedAt = e.ProcessedAt
            };
        }
    }
}
