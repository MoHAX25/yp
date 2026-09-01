using System.Text.Json.Serialization;

namespace yp.Models
{
    public class Booking
    {
        public Guid Id { get; }
        public Guid EventId { get; }
        public BookingStatus Status { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime? ProcessedAt { get; private set; }

        public Booking(Guid eventId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = BookingStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void Confirm(DateTime processedAt)
        {
            EnsurePending();
            Status = BookingStatus.Confirmed;
            ProcessedAt = processedAt;
        }

        public void Reject(DateTime processedAt)
        {
            EnsurePending();
            Status = BookingStatus.Rejected;
            ProcessedAt = processedAt;
        }

        private void EnsurePending()
        {
            if (Status != BookingStatus.Pending)
                throw new InvalidOperationException(
                    $"Бронь {Id} уже обработана (текущий статус: {Status}).");
        }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected
    }
}