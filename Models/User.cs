using System.ComponentModel.DataAnnotations;

namespace MaziyaInnPubVleis.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public DateTime LastLogin { get; internal set; }
    }

    public enum UserRole
    {
        Admin,
        Manager,
        Cashier,
        EventCoordinator
    }
}