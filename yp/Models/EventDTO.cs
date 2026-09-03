using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace yp.Models
{
    public record EventDTO
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public required string Title { get; set; }
        public required string Description { get; set; }
        [Required]
        public DateTime StartAt { get; set; }
        [Required]
        public DateTime EndAt { get; set; }
        [Required]
        public int? TotalSeats { get; set; }
        public int? AvailableSeats { get; set; }
    }
}
