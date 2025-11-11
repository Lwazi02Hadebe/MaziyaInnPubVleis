// CustomerService.cs
using MaziyaInnPubVleis.Data;
using MaziyaInnPubVleis.Models;
using Microsoft.EntityFrameworkCore;

namespace MaziyaInnPubVleis.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IOrderService _orderService;
        private readonly ISixPackService _sixPackService;

        public CustomerService(ApplicationDbContext context, IInventoryService inventoryService, IOrderService orderService, ISixPackService sixPackService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _orderService = orderService;
            _sixPackService = sixPackService;
        }

        public async Task<CustomerAccount> RegisterCustomerAsync(CustomerAccount customer)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if email already exists
                if (await _context.CustomerAccounts.AnyAsync(c => c.Email == customer.Email))
                {
                    throw new InvalidOperationException("Email already registered.");
                }

                // Create Customer record FIRST
                var customerRecord = new Customer
                {
                    Name = $"{customer.FirstName} {customer.LastName}",
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = "Not specified",
                    RegistrationDate = DateTime.Now
                };

                _context.Customers.Add(customerRecord);
                await _context.SaveChangesAsync();

                // Now create CustomerAccount with the CustomerId
                var customerAccount = new CustomerAccount
                {
                    Email = customer.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(customer.PasswordHash),
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Phone = customer.Phone,
                    CustomerId = customerRecord.CustomerId,
                    RegistrationDate = DateTime.Now,
                    IsActive = true,
                    LastLogin = DateTime.Now
                };

                _context.CustomerAccounts.Add(customerAccount);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return customerAccount;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Registration failed: {ex.Message}");
            }
        }

        public async Task<CustomerAccount> LoginCustomerAsync(string email, string password)
        {
            var customer = await _context.CustomerAccounts
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Email == email && c.IsActive);

            if (customer != null && BCrypt.Net.BCrypt.Verify(password, customer.PasswordHash))
            {
                customer.LastLogin = DateTime.Now;
                await _context.SaveChangesAsync();
                return customer;
            }

            throw new InvalidOperationException("Invalid email or password.");
        }

        public async Task<CustomerAccount> GetCustomerByIdAsync(int id)
        {
            return await _context.CustomerAccounts
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.CustomerAccountId == id && c.IsActive);
        }

        public async Task<bool> UpdateCustomerAsync(CustomerAccount customer)
        {
            var existingCustomer = await _context.CustomerAccounts.FindAsync(customer.CustomerAccountId);
            if (existingCustomer == null)
                return false;

            _context.Entry(existingCustomer).CurrentValues.SetValues(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Cart> GetOrCreateCartAsync(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Inventory)
                .FirstOrDefaultAsync(c => c.CustomerAccountId == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerAccountId = customerId,
                    CreatedDate = DateTime.Now,
                    LastUpdated = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartItem> AddToCartAsync(int customerId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than 0.");

            var cart = await GetOrCreateCartAsync(customerId);
            var inventory = await _inventoryService.GetInventoryByIdAsync(productId);

            if (inventory == null || !inventory.IsActive)
                throw new InvalidOperationException("Product not available.");

            // Calculate actual units needed (considering six-pack)
            int unitsNeeded = _sixPackService.CalculateUnitsToDeduct(quantity, inventory.IsSixPack, inventory.SixPackQuantity);

            if (inventory.StockLevel < unitsNeeded)
                throw new InvalidOperationException($"Insufficient stock. Only {inventory.StockLevel} units available.");

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            // Calculate unit price based on six-pack configuration
            decimal unitPrice;
            if (inventory.IsSixPack)
            {
                var metrics = _sixPackService.CalculateUnitMetrics(inventory);
                unitPrice = metrics.unitPrice;
            }
            else
            {
                unitPrice = inventory.UnitPrice;
            }

            if (existingItem != null)
            {
                // Check if updated quantity is available
                int newUnitsNeeded = _sixPackService.CalculateUnitsToDeduct(
                    existingItem.Quantity + quantity,
                    inventory.IsSixPack,
                    inventory.SixPackQuantity);

                if (inventory.StockLevel < newUnitsNeeded)
                    throw new InvalidOperationException($"Insufficient stock. Only {inventory.StockLevel} units available.");

                existingItem.Quantity += quantity;
                existingItem.UnitPrice = unitPrice;
            }
            else
            {
                existingItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    AddedDate = DateTime.Now
                };
                _context.CartItems.Add(existingItem);
            }

            cart.LastUpdated = DateTime.Now;
            await _context.SaveChangesAsync();
            return existingItem;
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
        {
            if (quantity <= 0)
                return await RemoveFromCartAsync(cartItemId);

            var cartItem = await _context.CartItems
                .Include(ci => ci.Inventory)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem != null)
            {
                // Calculate actual units needed
                int unitsNeeded = _sixPackService.CalculateUnitsToDeduct(
                    quantity,
                    cartItem.Inventory.IsSixPack,
                    cartItem.Inventory.SixPackQuantity);

                if (cartItem.Inventory.StockLevel < unitsNeeded)
                    throw new InvalidOperationException($"Insufficient stock. Only {cartItem.Inventory.StockLevel} units available.");

                // Update unit price if it's a six-pack item
                if (cartItem.Inventory.IsSixPack)
                {
                    var metrics = _sixPackService.CalculateUnitMetrics(cartItem.Inventory);
                    cartItem.UnitPrice = metrics.unitPrice;
                }

                cartItem.Quantity = quantity;
                cartItem.Cart.LastUpdated = DateTime.Now;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<Cart> GetCartWithItemsAsync(int customerId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Inventory)
                .FirstOrDefaultAsync(c => c.CustomerAccountId == customerId);
        }

        public async Task<bool> ClearCartAsync(int customerId)
        {
            var cart = await GetCartWithItemsAsync(customerId);
            if (cart != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.LastUpdated = DateTime.Now;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<CartSummary> GetCartSummaryAsync(int customerId)
        {
            var cart = await GetCartWithItemsAsync(customerId);
            var summary = new CartSummary();

            if (cart == null) return summary;

            foreach (var item in cart.CartItems)
            {
                var inventory = item.Inventory;
                decimal itemPrice;
                int actualUnits;

                if (inventory.IsSixPack)
                {
                    var metrics = _sixPackService.CalculateUnitMetrics(inventory);
                    itemPrice = metrics.unitPrice;
                    actualUnits = item.Quantity * inventory.SixPackQuantity;
                }
                else
                {
                    itemPrice = inventory.UnitPrice;
                    actualUnits = item.Quantity;
                }

                decimal itemTotal = item.Quantity * itemPrice;

                summary.Subtotal += itemTotal;
                summary.TotalItems += item.Quantity;

                summary.Items.Add(new CartItemSummary
                {
                    CartItemId = item.CartItemId,
                    ProductId = inventory.ProductId,
                    ProductName = inventory.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = itemPrice,
                    TotalPrice = itemTotal,
                    IsSixPack = inventory.IsSixPack,
                    SixPackQuantity = inventory.SixPackQuantity,
                    ActualUnits = actualUnits
                });
            }

            // Calculate VAT (15% in South Africa)
            summary.VAT = Math.Round(summary.Subtotal * 0.15m, 2);
            summary.Total = Math.Round(summary.Subtotal + summary.VAT, 2);

            return summary;
        }

        public async Task<Order> CreateOrderFromCartAsync(int customerId, int processedByUserId)
        {
            var cart = await GetCartWithItemsAsync(customerId);
            if (cart == null || !cart.CartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            var customerAccount = await GetCustomerByIdAsync(customerId);
            if (customerAccount?.CustomerId == null)
                throw new InvalidOperationException("Customer not found");

            // Convert cart items to order items
            var orderItems = cart.CartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                TotalPrice = ci.Quantity * ci.UnitPrice
            }).ToList();

            var order = new Order
            {
                CustomerId = customerAccount.CustomerId.Value,
                ProcessedByUserId = processedByUserId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending
            };

            // Use the order service to create the order with six-pack handling
            var createdOrder = await _orderService.ProcessOrderWithSixPackItemsAsync(order, orderItems);

            // Clear the cart after successful order creation
            await ClearCartAsync(customerId);

            return createdOrder;
        }

        public async Task<List<Order>> GetCustomerOrdersAsync(int customerId)
        {
            var customerAccount = await GetCustomerByIdAsync(customerId);
            if (customerAccount?.CustomerId == null)
                return new List<Order>();

            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Inventory)
                .Include(o => o.ProcessedBy)
                .Include(o => o.Customer)
                .Where(o => o.CustomerId == customerAccount.CustomerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetCustomerOrderDetailsAsync(int orderId, int customerId)
        {
            var customerAccount = await GetCustomerByIdAsync(customerId);
            if (customerAccount?.CustomerId == null)
                return null;

            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Inventory)
                .Include(o => o.ProcessedBy)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerAccount.CustomerId);
        }

        public async Task<List<Event>> GetUpcomingEventsAsync()
        {
            return await _context.Events
                .Include(e => e.CreatedBy)
                .Where(e => e.EventDate >= DateTime.Today && e.Status == EventStatus.Scheduled)
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<EventBooking> BookEventAsync(int eventId, int customerId, int numberOfTickets)
        {
            if (numberOfTickets <= 0)
                throw new InvalidOperationException("Number of tickets must be greater than 0.");

            var customerAccount = await GetCustomerByIdAsync(customerId);
            if (customerAccount?.CustomerId == null)
                throw new InvalidOperationException("Customer not found.");

            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null || eventItem.Status != EventStatus.Scheduled)
                throw new InvalidOperationException("Event not available for booking.");

            if (eventItem.CurrentAttendees + numberOfTickets > eventItem.MaxAttendees)
                throw new InvalidOperationException($"Not enough tickets available. Only {eventItem.MaxAttendees - eventItem.CurrentAttendees} tickets left.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var booking = new EventBooking
                {
                    EventId = eventId,
                    CustomerId = customerAccount.CustomerId.Value,
                    NumberOfTickets = numberOfTickets,
                    TotalAmount = numberOfTickets * eventItem.TicketPrice,
                    BookingDate = DateTime.Now,
                    Status = BookingStatus.Confirmed
                };

                eventItem.CurrentAttendees += numberOfTickets;

                _context.EventBookings.Add(booking);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return booking;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Event booking failed: {ex.Message}");
            }
        }

        public async Task<List<EventBooking>> GetCustomerEventBookingsAsync(int customerId)
        {
            var customerAccount = await GetCustomerByIdAsync(customerId);
            if (customerAccount?.CustomerId == null)
                return new List<EventBooking>();

            return await _context.EventBookings
                .Include(eb => eb.Event)
                .Where(eb => eb.CustomerId == customerAccount.CustomerId)
                .OrderByDescending(eb => eb.BookingDate)
                .ToListAsync();
        }

        public async Task<bool> CancelEventBookingAsync(int bookingId, int customerId)
        {
            var customerAccount = await GetCustomerByIdAsync(customerId);
            if (customerAccount?.CustomerId == null)
                return false;

            var booking = await _context.EventBookings
                .Include(eb => eb.Event)
                .FirstOrDefaultAsync(eb => eb.BookingId == bookingId && eb.CustomerId == customerAccount.CustomerId);

            if (booking == null || booking.Status == BookingStatus.Cancelled)
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                booking.Status = BookingStatus.Cancelled;
                booking.Event.CurrentAttendees -= booking.NumberOfTickets;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}