using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaziyaInnPubVleis.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int PurchaseOrderId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? ExpectedDeliveryDate { get; set; }

        [Required]
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        // Foreign keys
        public int SupplierId { get; set; }
        public int CreatedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User CreatedBy { get; set; }

        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public ICollection<GoodsReceivedVoucher> GoodsReceivedVouchers { get; set; }
    }

    public enum PurchaseOrderStatus
    {
        Pending,
        Ordered,
        PartiallyReceived,
        Completed,
        Cancelled
    }
}