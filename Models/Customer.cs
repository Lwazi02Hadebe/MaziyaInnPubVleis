using MaziyaInnPubVleis.Data;
using System.ComponentModel.DataAnnotations;

namespace MaziyaInnPubVleis.Models
{
    public class Customer : IHasTimestamps
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Address { get; set; } = "Not specified";

        [StringLength(15)]
        public string Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<Order> Orders { get; set; }
        public ICollection<EventBooking> EventBookings { get; set; }
    }
}