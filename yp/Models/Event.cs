using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace yp.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

    }
}
