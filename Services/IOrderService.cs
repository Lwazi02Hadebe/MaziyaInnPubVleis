// IOrderService.cs
using MaziyaInnPubVleis.Models;

namespace MaziyaInnPubVleis.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int id);
        Task<Order> CreateOrderAsync(Order order, List<OrderItem> orderItems);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<SalesReport> GenerateSalesReportAsync(DateTime startDate, DateTime endDate);
        Task<bool> CancelOrderAsync(int orderId);

        // New methods for six-pack integration
        Task<Order> ProcessOrderWithSixPackItemsAsync(Order order, List<OrderItem> orderItems);
        Task<decimal> CalculateOrderTotalWithSixPacksAsync(List<OrderItem> orderItems);
        Task<OrderSummary> CalculateOrderSummaryAsync(List<OrderItem> orderItems);

        // New method for manager dashboard
        Task<decimal> GetDailySalesTotalAsync(DateTime date);
    }

    public class SalesReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalVAT { get; set; }
        public decimal TotalGrossProfit { get; set; }
        public decimal AverageProfitMargin { get; set; }
        public int TotalOrders { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<ProductSales> TopSellingProducts { get; set; } = new List<ProductSales>();
    }

    public class ProductSales
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public bool IsSixPack { get; set; }
        public int ActualUnitsSold { get; set; }
    }

    public class OrderSummary
    {
        public decimal Subtotal { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public List<OrderItemSummary> Items { get; set; } = new List<OrderItemSummary>();
    }

    public class OrderItemSummary
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsSixPack { get; set; }
        public int SixPackQuantity { get; set; }
        public int ActualUnits { get; set; }
    }
}