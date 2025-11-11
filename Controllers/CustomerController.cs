
using Microsoft.AspNetCore.Mvc;
using MaziyaInnPubVleis.Models;
using MaziyaInnPubVleis.Services;
using Microsoft.EntityFrameworkCore;

namespace MaziyaInnPubVleis.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IInventoryService _inventoryService;
        private readonly ISixPackService _sixPackService;
        private readonly IOrderService _orderService;

        public CustomerController(ICustomerService customerService, IInventoryService inventoryService, ISixPackService sixPackService, IOrderService orderService)
        {
            _customerService = customerService;
            _inventoryService = inventoryService;
            _sixPackService = sixPackService;
            _orderService = orderService;
        }

        // Customer Authentication
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("CustomerId") != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    TempData["ErrorMessage"] = "Please enter both email and password.";
                    return View();
                }

                var customer = await _customerService.LoginCustomerAsync(email, password);

                HttpContext.Session.SetInt32("CustomerId", customer.CustomerAccountId);
                HttpContext.Session.SetString("CustomerName", $"{customer.FirstName} {customer.LastName}");
                HttpContext.Session.SetString("CustomerEmail", customer.Email);

                TempData["SuccessMessage"] = $"Welcome back, {customer.FirstName}!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
        }

        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("CustomerId") != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(CustomerAccount customer, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(customer.PasswordHash) || string.IsNullOrEmpty(confirmPassword))
                {
                    TempData["ErrorMessage"] = "Please enter and confirm your password.";
                    return View(customer);
                }

                if (customer.PasswordHash != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Passwords do not match.";
                    return View(customer);
                }

                var newCustomer = await _customerService.RegisterCustomerAsync(customer);

                HttpContext.Session.SetInt32("CustomerId", newCustomer.CustomerAccountId);
                HttpContext.Session.SetString("CustomerName", $"{newCustomer.FirstName} {newCustomer.LastName}");
                HttpContext.Session.SetString("CustomerEmail", newCustomer.Email);

                TempData["SuccessMessage"] = $"Welcome, {newCustomer.FirstName}! Your account has been created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(customer);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            try
            {
                var customerName = HttpContext.Session.GetString("CustomerName");
                HttpContext.Session.Clear();
                TempData["SuccessMessage"] = $"Goodbye, {customerName}! You have been logged out successfully.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error during logout.";
                return RedirectToAction("Login");
            }
        }

        // Customer Dashboard
        public async Task<IActionResult> Index()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var inventory = await _inventoryService.GetAllInventoryAsync();
            var upcomingEvents = await _customerService.GetUpcomingEventsAsync();
            var recentOrders = await _customerService.GetCustomerOrdersAsync(customerId.Value);

            ViewBag.UpcomingEvents = upcomingEvents;
            ViewBag.RecentOrders = recentOrders.Take(5).ToList();
            ViewBag.SixPackService = _sixPackService;

            // Get customer details for dashboard
            var customer = await _customerService.GetCustomerByIdAsync(customerId.Value);
            ViewBag.Customer = customer?.Customer;

            return View(inventory);
        }

        // Shopping Cart
        public async Task<IActionResult> Cart()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var cart = await _customerService.GetCartWithItemsAsync(customerId.Value);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                if (customerId == null)
                {
                    return Json(new { success = false, message = "Please login first." });
                }

                await _customerService.AddToCartAsync(customerId.Value, productId, quantity);
                return Json(new { success = true, message = "Product added to cart." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            try
            {
                await _customerService.UpdateCartItemQuantityAsync(cartItemId, quantity);
                return Json(new { success = true, message = "Cart updated." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                await _customerService.RemoveFromCartAsync(cartItemId);
                return Json(new { success = true, message = "Item removed from cart." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Checkout
        public async Task<IActionResult> Checkout()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var cart = await _customerService.GetCartWithItemsAsync(customerId.Value);
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Cart");
            }

            // Get customer details for checkout
            var customer = await _customerService.GetCustomerByIdAsync(customerId.Value);
            ViewBag.Customer = customer?.Customer;

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            try
            {
                var order = await _customerService.CreateOrderFromCartAsync(customerId.Value, 3);
                TempData["SuccessMessage"] = $"Order #{order.OrderId} placed successfully!";
                return RedirectToAction("OrderConfirmation", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error placing order: {ex.Message}";
                return RedirectToAction("Checkout");
            }
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var order = await _customerService.GetCustomerOrderDetailsAsync(id, customerId.Value);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Orders");
            }

            return View(order);
        }

        // Orders Management
        public async Task<IActionResult> Orders()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var orders = await _customerService.GetCustomerOrdersAsync(customerId.Value);
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var order = await _customerService.GetCustomerOrderDetailsAsync(id, customerId.Value);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Orders");
            }

            return View(order);
        }

        // Events Management
        public async Task<IActionResult> Events()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var events = await _customerService.GetUpcomingEventsAsync();
            return View(events);
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var events = await _customerService.GetUpcomingEventsAsync();
            var eventItem = events.FirstOrDefault(e => e.EventId == id);
            if (eventItem == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction("Events");
            }

            var relatedEvents = events.Where(e => e.EventId != id).Take(3).ToList();
            ViewBag.RelatedEvents = relatedEvents;

            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookEvent(int eventId, int numberOfTickets)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                if (customerId == null)
                {
                    return Json(new { success = false, message = "Please login first." });
                }

                var booking = await _customerService.BookEventAsync(eventId, customerId.Value, numberOfTickets);
                return Json(new { success = true, message = "Event booked successfully!", bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> MyBookings()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var bookings = await _customerService.GetCustomerEventBookingsAsync(customerId.Value);
            return View(bookings);
        }

        // Receipt and Ticket Generation
        public async Task<IActionResult> OrderReceipt(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var order = await _customerService.GetCustomerOrderDetailsAsync(id, customerId.Value);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Orders");
            }

            ViewBag.PrintMode = true;
            return View("OrderReceipt", order);
        }

        public async Task<IActionResult> EventTicket(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var bookings = await _customerService.GetCustomerEventBookingsAsync(customerId.Value);
            var booking = bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("MyBookings");
            }

            ViewBag.PrintMode = true;
            return View("EventTicket", booking);
        }

        // Download PDF versions
        public async Task<IActionResult> DownloadOrderReceipt(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var order = await _customerService.GetCustomerOrderDetailsAsync(id, customerId.Value);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Orders");
            }

            // For PDF generation, you would use a library like iTextSharp
            // For now, we'll return the view
            ViewBag.PrintMode = true;
            return View("OrderReceipt", order);
        }

        public async Task<IActionResult> DownloadEventTicket(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var bookings = await _customerService.GetCustomerEventBookingsAsync(customerId.Value);
            var booking = bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("MyBookings");
            }

            ViewBag.PrintMode = true;
            return View("EventTicket", booking);
        }

        // Utility Methods
        [HttpGet]
        public async Task<JsonResult> GetCartCount()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return Json(new { count = 0 });
            }

            var cart = await _customerService.GetCartWithItemsAsync(customerId.Value);
            var count = cart?.CartItems?.Sum(ci => ci.Quantity) ?? 0;
            return Json(new { count });
        }
    }
}
