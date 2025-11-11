using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaziyaInnPubVleis.Models
{
    public class GRVItem
    {
        [Key]
        public int GRVItemId { get; set; }

        [Required]
        public int QuantityReceived { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        // Foreign keys
        public int GRVId { get; set; }
        public int ProductId { get; set; }

        // Navigation properties
        [ForeignKey("GRVId")]
        public GoodsReceivedVoucher GoodsReceivedVoucher { get; set; }

        [ForeignKey("ProductId")]
        public Inventory Inventory { get; set; }
    }
}