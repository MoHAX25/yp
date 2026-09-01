using System.Collections.Concurrent;
using yp.Exceptions;
using yp.Models;

namespace yp.Services
{
    public class BookingService : IBookingService
    {
        private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();
        private readonly IEventService _eventService;

        public BookingService(IEventService eventService)
        {
            _eventService = eventService;
        }
        public Task<Booking> CreateBookingAsync(Guid eventId)
        {
            _ = _eventService.Get(eventId)
                ?? throw new NotFoundException($"Событие с id {eventId} не найдено.");

            var booking = new Booking(eventId);
            _bookings[booking.Id] = booking;
            return Task.FromResult(booking);
        }

        public Task<Booking?> GetBookingByIdAsync(Guid bookingId)
        {
            _bookings.TryGetValue(bookingId, out var booking);
            return Task.FromResult(booking);
        }

        public Task<List<Booking>> GetPendingBookingsAsync()
        {
            var pending = _bookings.Values
                .Where(b => b.Status == BookingStatus.Pending)
                .ToList();
            return Task.FromResult(pending);
        }

        public Task UpdateBookingAsync(Booking booking)
        {
            _bookings[booking.Id] = booking;
            return Task.CompletedTask;
        }
    }
}
