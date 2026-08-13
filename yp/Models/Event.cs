using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace yp.Models
{
    public record Event
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

    }
}
