using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaziyaInnPubVleis.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string EventName { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public EventStatus Status { get; set; } = EventStatus.Scheduled;

        [Range(0, int.MaxValue)]
        public int MaxAttendees { get; set; }

        [Range(0, int.MaxValue)]
        public int CurrentAttendees { get; set; } = 0;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TicketPrice { get; set; }

        // Foreign keys
        public int CreatedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public User CreatedBy { get; set; }

        public ICollection<EventBooking> Bookings { get; set; }
        public ICollection<EventStock> EventStocks { get; set; }
    }

    public enum EventStatus
    {
        Scheduled,
        Ongoing,
        Completed,
        Cancelled
    }
}