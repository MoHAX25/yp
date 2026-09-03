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
        [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Booking>> Get(Guid id)
        {
            var booking = await _service.GetBookingByIdAsync(id);

            if (booking == null)
                throw new NotFoundException($"Бронь с id {id} не найдена.");

            return Ok(ToDto(booking));
        }

        /// <summary>
        /// Создать бронь для события
        /// </summary>
        /// <param name="id">ID события</param>
        /// <returns>
        /// 202 Accepted - бронь создана успешно
        /// 404 Not Found - событие не найдено
        /// 409 Conflict - нет доступных мест
        /// </returns>
        [HttpPost("/events/{id}/book")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDTO>> Book(Guid id)
        {
            var booking = await _service.CreateBookingAsync(id);

            var location = $"/bookings/{booking.Id}";

            return Accepted(location, ToDto(booking));
        }

        [HttpPost("/bookings/{id}/reject")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDTO>> Reject(Guid id)
        {
            var booking = await _service.RejectBookingAsync(id);
            return Ok(ToDto(booking));
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
