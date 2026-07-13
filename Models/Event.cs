using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace yp.Models
{
    public class Event
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

    }
}
