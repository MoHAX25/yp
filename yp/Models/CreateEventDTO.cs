using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace yp.Models
{
    public record CreateEventDTO : IValidatableObject
    {
        [Required]
        public required string Title { get; set; }
        public required string Description { get; set; }
        [Required]
        public DateTime StartAt { get; set; }
        [Required]
        public DateTime EndAt { get; set; }
        [Required]
        public int? TotalSeats { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndAt <= StartAt)
            {
                yield return new ValidationResult("EndAt должен быть позже StartAt.", new[] { nameof(EndAt), nameof(StartAt) });
            }
            if (TotalSeats <= 0)
                yield return new ValidationResult("TotalSeats должно быть больше нуля.", new[] { nameof(TotalSeats) });
        }
    }
}
