using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaziyaInnPubVleis.Models
{
    public class EventStock
    {
        [Key]
        public int EventStockId { get; set; }

        [Required]
        public int QuantityAllocated { get; set; }

        public int QuantityUsed { get; set; } = 0;

        // Foreign keys
        public int EventId { get; set; }
        public int ProductId { get; set; }

        // Navigation properties
        [ForeignKey("EventId")]
        public Event Event { get; set; }

        [ForeignKey("ProductId")]
        public Inventory Inventory { get; set; }
    }
}