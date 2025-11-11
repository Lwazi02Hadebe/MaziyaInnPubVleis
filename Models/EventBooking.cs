using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaziyaInnPubVleis.Models
{
    public class EventBooking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        public int NumberOfTickets { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

        // Foreign keys - Use CustomerId
        public int EventId { get; set; }
        public int CustomerId { get; set; }

        // Navigation properties
        [ForeignKey("EventId")]
        public Event Event { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Attended
    }
}