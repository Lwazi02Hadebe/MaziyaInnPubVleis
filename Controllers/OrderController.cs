using Microsoft.AspNetCore.Mvc;
using MaziyaInnPubVleis.Models;
using MaziyaInnPubVleis.Services;
using System.Text.Json;

namespace MaziyaInnPubVleis.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IInventoryService _inventoryService;

        public OrderController(IOrderService orderService, IInventoryService inventoryService)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> Create()
        {
            var inventory = await _inventoryService.GetAllInventoryAsync();
            ViewBag.Inventory = inventory;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, string orderItemsJson)
        {
            try
            {
                if (string.IsNullOrEmpty(orderItemsJson))
                {
                    ModelState.AddModelError("", "Please add items to the order.");
                    var inventory = await _inventoryService.GetAllInventoryAsync();
                    ViewBag.Inventory = inventory;
                    return View(order);
                }

                var orderItems = JsonSerializer.Deserialize<List<OrderItem>>(orderItemsJson);

                if (orderItems == null || !orderItems.Any())
                {
                    ModelState.AddModelError("", "Please add items to the order.");
                    var inventory = await _inventoryService.GetAllInventoryAsync();
                    ViewBag.Inventory = inventory;
                    return View(order);
                }

                // Set order date and status
                order.OrderDate = DateTime.Now;
                order.Status = OrderStatus.Pending;

                // Create the order
                var createdOrder = await _orderService.CreateOrderAsync(order, orderItems);
                return RedirectToAction(nameof(Details), new { id = createdOrder.OrderId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating order: {ex.Message}");
                var inventory = await _inventoryService.GetAllInventoryAsync();
                ViewBag.Inventory = inventory;
                return View(order);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, status);
            if (success)
            {
                TempData["SuccessMessage"] = "Order status updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update order status.";
            }
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}