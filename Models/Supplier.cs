using System.ComponentModel.DataAnnotations;

namespace MaziyaInnPubVleis.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(100)]
        public string ContactPerson { get; set; }

        [StringLength(15)]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        // Navigation properties
        public ICollection<Inventory> InventoryItems { get; set; }
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
    }
}
