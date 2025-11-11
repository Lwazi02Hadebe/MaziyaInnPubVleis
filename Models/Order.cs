using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaziyaInnPubVleis.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal VATAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal GrossProfit { get; set; }

        public decimal ProfitMargin => TotalAmount > 0 ? (GrossProfit / TotalAmount) * 100 : 0;

        // Payment information
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public string? PaymentReference { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? RefundAmount { get; set; }
        public DateTime? RefundDate { get; set; }
        public string? CancellationReason { get; set; }

        // Foreign keys - Use CustomerId (not CustomerAccountId)
        public int? CustomerId { get; set; }
        public int ProcessedByUserId { get; set; }
        public int? ApprovedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [ForeignKey("ProcessedByUserId")]
        public User ProcessedBy { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public User ApprovedBy { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Approved,
        Completed,
        Cancelled,
        Refunded
    }

    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        DebitCard,
        EFT
    }
}