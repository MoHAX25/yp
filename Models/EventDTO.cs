using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace yp.Models
{
    public record EventDTO : IValidatableObject
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime StartAt { get; set; }
        [Required]
        public DateTime EndAt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndAt <= StartAt)
            {
                yield return new ValidationResult("EndAt должен быть позже StartAt.", new[] { nameof(EndAt), nameof(StartAt) });
            }
        }

    }
}
