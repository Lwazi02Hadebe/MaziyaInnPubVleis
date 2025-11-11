using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaziyaInnPubVleis.Models
{
    public class GoodsReceivedVoucher
    {
        [Key]
        public int GRVId { get; set; }

        [Required]
        public DateTime ReceivedDate { get; set; } = DateTime.Now;

        [Required]
        public string ReferenceNumber { get; set; }

        [Required]
        public int PurchaseOrderId { get; set; }
        public int ReceivedByUserId { get; set; }
        public int? AuthorizedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("PurchaseOrderId")]
        public PurchaseOrder PurchaseOrder { get; set; }

        [ForeignKey("ReceivedByUserId")]
        public User ReceivedBy { get; set; }

        [ForeignKey("AuthorizedByUserId")]
        public User AuthorizedBy { get; set; }

        public ICollection<GRVItem> GRVItems { get; set; }
    }
}
