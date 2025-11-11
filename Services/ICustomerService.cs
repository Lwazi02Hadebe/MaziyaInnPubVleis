// ICustomerService.cs
using MaziyaInnPubVleis.Models;

namespace MaziyaInnPubVleis.Services
{
    public interface ICustomerService
    {
        Task<CustomerAccount> RegisterCustomerAsync(CustomerAccount customer);
        Task<CustomerAccount> LoginCustomerAsync(string email, string password);
        Task<CustomerAccount> GetCustomerByIdAsync(int id);
        Task<bool> UpdateCustomerAsync(CustomerAccount customer);

        // Cart management
        Task<Cart> GetOrCreateCartAsync(int customerId);
        Task<CartItem> AddToCartAsync(int customerId, int productId, int quantity);
        Task<bool> RemoveFromCartAsync(int cartItemId);
        Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity);
        Task<Cart> GetCartWithItemsAsync(int customerId);
        Task<bool> ClearCartAsync(int customerId);
        Task<CartSummary> GetCartSummaryAsync(int customerId);

        // Orders
        Task<Order> CreateOrderFromCartAsync(int customerId, int processedByUserId);
        Task<List<Order>> GetCustomerOrdersAsync(int customerId);
        Task<Order> GetCustomerOrderDetailsAsync(int orderId, int customerId);

        // Events
        Task<List<Event>> GetUpcomingEventsAsync();
        Task<EventBooking> BookEventAsync(int eventId, int customerId, int numberOfTickets);
        Task<List<EventBooking>> GetCustomerEventBookingsAsync(int customerId);
        Task<bool> CancelEventBookingAsync(int bookingId, int customerId);
    }

    public class CartSummary
    {
        public int TotalItems { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VAT { get; set; }
        public decimal Total { get; set; }
        public List<CartItemSummary> Items { get; set; } = new List<CartItemSummary>();
    }

    public class CartItemSummary
    {
        public int CartItemId { get; set; }
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