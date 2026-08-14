namespace yp.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected
    }
}
