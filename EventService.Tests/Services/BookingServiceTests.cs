using yp.Exceptions;
using yp.Models;
using yp.Services;

namespace yp.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly InMemoryEventStore _eventStore;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _eventStore = new InMemoryEventStore();
            _bookingService = new BookingService(_eventStore);
        }

        private static Event CreateTestEvent(Guid id, int totalSeats = 100)
        {
            return new Event(totalSeats)
            {
                Id = id,
                Title = "Test event",
                Description = "Test description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(1).AddHours(2)
            };
        }

        private async Task<BookingAttempt[]> RunConcurrentBookingAttemptsAsync(
            Guid eventId,
            int requestCount)
        {
            var startGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var tasks = Enumerable.Range(0, requestCount)
                .Select(_ => Task.Run(async () =>
                {
                    await startGate.Task;

                    try
                    {
                        var booking = await _bookingService.CreateBookingAsync(eventId);
                        return BookingAttempt.Success(booking);
                    }
                    catch (Exception ex)
                    {
                        return BookingAttempt.Failure(ex);
                    }
                }))
                .ToArray();

            startGate.SetResult();

            return await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldReturnPendingBooking_WhenEventExists()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId));

            var booking = await _bookingService.CreateBookingAsync(eventId);

            Assert.NotEqual(Guid.Empty, booking.Id);
            Assert.Equal(eventId, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Null(booking.ProcessedAt);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldDecreaseAvailableSeatsByOne_WhenBookingCreated()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId, totalSeats: 3));

            await _bookingService.CreateBookingAsync(eventId);

            var updatedEvent = _eventStore.Get(eventId);

            Assert.NotNull(updatedEvent);
            Assert.Equal(2, updatedEvent!.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldCreateBookingsUpToSeatLimit_WithUniqueIds()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId, totalSeats: 3));

            var bookings = new[]
            {
                await _bookingService.CreateBookingAsync(eventId),
                await _bookingService.CreateBookingAsync(eventId),
                await _bookingService.CreateBookingAsync(eventId)
            };

            Assert.Equal(3, bookings.Select(b => b.Id).Distinct().Count());
            Assert.All(bookings, booking => Assert.Equal(eventId, booking.EventId));

            var updatedEvent = _eventStore.Get(eventId);
            Assert.NotNull(updatedEvent);
            Assert.Equal(0, updatedEvent!.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldThrowNoAvailableSeatsException_AfterSeatsAreExhausted()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId, totalSeats: 2));

            await _bookingService.CreateBookingAsync(eventId);
            await _bookingService.CreateBookingAsync(eventId);

            await Assert.ThrowsAsync<NoAvailableSeatsException>(
                () => _bookingService.CreateBookingAsync(eventId));
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldThrowNotFoundException_WhenEventDoesNotExist()
        {
            var eventId = Guid.NewGuid();

            await Assert.ThrowsAsync<NotFoundException>(
                () => _bookingService.CreateBookingAsync(eventId));
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldThrowNoAvailableSeatsException_WhenNoSeatsAvailable()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId, totalSeats: 1));

            await _bookingService.CreateBookingAsync(eventId);

            await Assert.ThrowsAsync<NoAvailableSeatsException>(
                () => _bookingService.CreateBookingAsync(eventId));
        }

        [Fact]
        public async Task GetBookingByIdAsync_ShouldReturnExistingBooking()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId));
            var created = await _bookingService.CreateBookingAsync(eventId);

            var fetched = await _bookingService.GetBookingByIdAsync(created.Id);

            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched!.Id);
            Assert.Equal(created.EventId, fetched.EventId);
            Assert.Equal(created.Status, fetched.Status);
        }

        [Fact]
        public async Task GetBookingByIdAsync_ShouldReturnNull_WhenBookingDoesNotExist()
        {
            var result = await _bookingService.GetBookingByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateBookingAsync_ShouldPersistConfirmedStatusAndProcessedAt()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId));
            var created = await _bookingService.CreateBookingAsync(eventId);
            var processedAt = DateTime.UtcNow;

            var booking = await _bookingService.GetBookingByIdAsync(created.Id);
            Assert.NotNull(booking);

            booking!.Confirm(processedAt);
            await _bookingService.UpdateBookingAsync(booking);

            var updated = await _bookingService.GetBookingByIdAsync(created.Id);

            Assert.NotNull(updated);
            Assert.Equal(BookingStatus.Confirmed, updated!.Status);
            Assert.Equal(processedAt, updated.ProcessedAt);
        }

        [Fact]
        public async Task UpdateBookingAsync_ShouldPersistRejectedStatusAndProcessedAt()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId));
            var created = await _bookingService.CreateBookingAsync(eventId);
            var processedAt = DateTime.UtcNow;

            var booking = await _bookingService.GetBookingByIdAsync(created.Id);
            Assert.NotNull(booking);

            booking!.Reject(processedAt);
            await _bookingService.UpdateBookingAsync(booking);

            var updated = await _bookingService.GetBookingByIdAsync(created.Id);

            Assert.NotNull(updated);
            Assert.Equal(BookingStatus.Rejected, updated!.Status);
            Assert.Equal(processedAt, updated.ProcessedAt);
        }

        [Fact]
        public async Task ReleaseSeats_ShouldRestoreAvailableSeats_AfterRejectedBooking()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId, totalSeats: 1));
            var created = await _bookingService.CreateBookingAsync(eventId);

            var booking = await _bookingService.GetBookingByIdAsync(created.Id);
            var reservedEvent = _eventStore.Get(eventId);

            Assert.NotNull(booking);
            Assert.NotNull(reservedEvent);
            Assert.Equal(0, reservedEvent!.AvailableSeats);

            booking!.Reject(DateTime.UtcNow);
            reservedEvent.ReleaseSeats();
            await _bookingService.UpdateBookingAsync(booking);
            _eventStore.Update(eventId, reservedEvent);

            var updatedEvent = _eventStore.Get(eventId);

            Assert.NotNull(updatedEvent);
            Assert.Equal(1, updatedEvent!.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldAllowNewBooking_AfterRejectedBookingReleasesSeat()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId, totalSeats: 1));
            var firstBooking = await _bookingService.CreateBookingAsync(eventId);

            var booking = await _bookingService.GetBookingByIdAsync(firstBooking.Id);
            var reservedEvent = _eventStore.Get(eventId);

            Assert.NotNull(booking);
            Assert.NotNull(reservedEvent);

            booking!.Reject(DateTime.UtcNow);
            reservedEvent!.ReleaseSeats();
            await _bookingService.UpdateBookingAsync(booking);
            _eventStore.Update(eventId, reservedEvent);

            var newBooking = await _bookingService.CreateBookingAsync(eventId);
            var updatedEvent = _eventStore.Get(eventId);

            Assert.NotEqual(firstBooking.Id, newBooking.Id);
            Assert.Equal(eventId, newBooking.EventId);
            Assert.NotNull(updatedEvent);
            Assert.Equal(0, updatedEvent!.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldPreventOverbooking_UnderConcurrentRequests()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId, totalSeats: 5));

            var attempts = await RunConcurrentBookingAttemptsAsync(eventId, requestCount: 20);

            var successfulBookings = attempts
                .Where(attempt => attempt.Booking is not null)
                .Select(attempt => attempt.Booking!)
                .ToList();
            var noSeatFailures = attempts
                .Where(attempt => attempt.Exception is NoAvailableSeatsException)
                .ToList();

            Assert.Equal(5, successfulBookings.Count);
            Assert.Equal(15, noSeatFailures.Count);
            Assert.All(
                attempts.Where(attempt => attempt.Exception is not null),
                attempt => Assert.IsType<NoAvailableSeatsException>(attempt.Exception));

            var updatedEvent = _eventStore.Get(eventId);
            Assert.NotNull(updatedEvent);
            Assert.Equal(0, updatedEvent!.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldGenerateUniqueIds_UnderConcurrentRequests()
        {
            var eventId = Guid.NewGuid();
            _eventStore.Add(CreateTestEvent(eventId, totalSeats: 10));

            var attempts = await RunConcurrentBookingAttemptsAsync(eventId, requestCount: 10);

            var bookings = attempts
                .Where(attempt => attempt.Booking is not null)
                .Select(attempt => attempt.Booking!)
                .ToList();

            Assert.Equal(10, bookings.Count);
            Assert.All(attempts, attempt => Assert.Null(attempt.Exception));
            Assert.Equal(10, bookings.Select(booking => booking.Id).Distinct().Count());

            var updatedEvent = _eventStore.Get(eventId);
            Assert.NotNull(updatedEvent);
            Assert.Equal(0, updatedEvent!.AvailableSeats);
        }

        private sealed record BookingAttempt(Booking? Booking, Exception? Exception)
        {
            public static BookingAttempt Success(Booking booking) => new(booking, null);

            public static BookingAttempt Failure(Exception exception) => new(null, exception);
        }
    }
}
