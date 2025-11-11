using MaziyaInnPubVleis.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaziyaInnPubVleis.Models
{
    public class CustomerAccount : IHasTimestamps
    {
        [Key]
        public int CustomerAccountId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(15)]
        [Phone]
        public string Phone { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public DateTime LastLogin { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Link to Customer record
        public int? CustomerId { get; set; }

        // Navigation properties
        public Customer Customer { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<EventBooking> EventBookings { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}