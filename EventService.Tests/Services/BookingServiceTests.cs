using Moq;
using yp.Exceptions;
using yp.Models;
using yp.Services;

namespace yp.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _eventServiceMock = new Mock<IEventService>();
            _bookingService = new BookingService(_eventServiceMock.Object);
        }

        private static Event CreateTestEvent(Guid id)
        {
            return new Event
            {
                Id = id,
                Title = "Test event",
                Description = "Test description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(1).AddHours(2)
            };
        }

        //Успешные сценарии

        [Fact]
        public async Task CreateBookingAsync_EventExists_ReturnsBookingWithPendingStatus()
        {
            var eventId = Guid.NewGuid();
            _eventServiceMock.Setup(s => s.Get(eventId)).Returns(CreateTestEvent(eventId));

            var booking = await _bookingService.CreateBookingAsync(eventId);

            Assert.NotEqual(Guid.Empty, booking.Id);
            Assert.Equal(eventId, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Null(booking.ProcessedAt);
        }

        [Fact]
        public async Task CreateBookingAsync_MultipleBookingsForSameEvent_AllHaveUniqueIds()
        {
            var eventId = Guid.NewGuid();
            _eventServiceMock.Setup(s => s.Get(eventId)).Returns(CreateTestEvent(eventId));

            var booking1 = await _bookingService.CreateBookingAsync(eventId);
            var booking2 = await _bookingService.CreateBookingAsync(eventId);
            var booking3 = await _bookingService.CreateBookingAsync(eventId);

            var ids = new[] { booking1.Id, booking2.Id, booking3.Id };

            Assert.Equal(3, ids.Distinct().Count());
            Assert.All(
                new[] { booking1, booking2, booking3 },
                b => Assert.Equal(eventId, b.EventId));
        }

        [Fact]
        public async Task GetBookingByIdAsync_ExistingBooking_ReturnsCorrectBooking()
        {
            var eventId = Guid.NewGuid();
            _eventServiceMock.Setup(s => s.Get(eventId)).Returns(CreateTestEvent(eventId));
            var created = await _bookingService.CreateBookingAsync(eventId);

            var fetched = await _bookingService.GetBookingByIdAsync(created.Id);

            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched!.Id);
            Assert.Equal(created.EventId, fetched.EventId);
            Assert.Equal(created.Status, fetched.Status);
        }

        [Fact]
        public async Task GetBookingByIdAsync_AfterStatusChangedToConfirmed_ReflectsNewStatus()
        {
            var eventId = Guid.NewGuid();
            _eventServiceMock.Setup(s => s.Get(eventId)).Returns(CreateTestEvent(eventId));
            var created = await _bookingService.CreateBookingAsync(eventId);

            var booking = await _bookingService.GetBookingByIdAsync(created.Id);
            booking?.Confirm(DateTime.Now);
            await _bookingService.UpdateBookingAsync(booking);

            var fetched = await _bookingService.GetBookingByIdAsync(created.Id);

            Assert.NotNull(fetched);
            Assert.Equal(BookingStatus.Confirmed, fetched!.Status);
            Assert.NotNull(fetched.ProcessedAt);
        }

        [Fact]
        public async Task GetBookingByIdAsync_AfterStatusChangedToRejected_ReflectsNewStatus()
        {
            var eventId = Guid.NewGuid();
            _eventServiceMock.Setup(s => s.Get(eventId)).Returns(CreateTestEvent(eventId));
            var created = await _bookingService.CreateBookingAsync(eventId);

            var booking = await _bookingService.GetBookingByIdAsync(created.Id);
            booking?.Reject(DateTime.Now);
            await _bookingService.UpdateBookingAsync(booking);

            var fetched = await _bookingService.GetBookingByIdAsync(created.Id);

            Assert.NotNull(fetched);
            Assert.Equal(BookingStatus.Rejected, fetched!.Status);
            Assert.NotNull(fetched.ProcessedAt);
        }

        // ---------- Неуспешные сценарии ----------

        [Fact]
        public async Task CreateBookingAsync_EventDoesNotExist_ThrowsNotFoundException()
        {
            var eventId = Guid.NewGuid();
            _eventServiceMock.Setup(s => s.Get(eventId)).Returns((Event?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _bookingService.CreateBookingAsync(eventId));
        }

        [Fact]
        public async Task CreateBookingAsync_EventDeleted_ThrowsNotFoundException()
        {
            var deletedEventId = Guid.NewGuid();
            _eventServiceMock.Setup(s => s.Get(deletedEventId)).Returns((Event?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _bookingService.CreateBookingAsync(deletedEventId));
        }

        [Fact]
        public async Task GetBookingByIdAsync_NonExistingId_ReturnsNull()
        {
            var result = await _bookingService.GetBookingByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }
    }
}