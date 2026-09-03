using System.Collections.Concurrent;
using yp.Exceptions;
using yp.Models;

namespace yp.Services
{
    public class BookingService : IBookingService
    {
        private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();
        private readonly InMemoryEventStore _eventStore;
        private readonly object _bookingLock = new();

        public BookingService(InMemoryEventStore eventStore)
        {
            _eventStore = eventStore;
        }
        public Task<Booking> CreateBookingAsync(Guid eventId)
        {
            lock (_bookingLock)
            {
                var @event = _eventStore.Get(eventId)
                    ?? throw new NotFoundException($"Событие с id {eventId} не найдено.");

                if (!@event.TryReserveSeats())
                {
                    throw new NoAvailableSeatsException("No available seats for this event");
                }

                _eventStore.Update(eventId, @event);

                var booking = new Booking(eventId);
                _bookings[booking.Id] = booking;
                return Task.FromResult(booking);
            }
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
