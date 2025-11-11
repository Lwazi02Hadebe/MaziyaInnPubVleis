using Microsoft.AspNetCore.Mvc;
using MaziyaInnPubVleis.Models;
using MaziyaInnPubVleis.Services;
using MaziyaInnPubVleis.Data;
using Microsoft.EntityFrameworkCore;

namespace MaziyaInnPubVleis.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;
        private readonly IInventoryService _inventoryService;
        private readonly IReportService _reportService;

        public ManagerController(ApplicationDbContext context, IOrderService orderService,
                               IInventoryService inventoryService, IReportService reportService)
        {
            _context = context;
            _orderService = orderService;
            _inventoryService = inventoryService;
            _reportService = reportService;
        }

        // Manager Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var dashboard = new ManagerDashboardViewModel
            {
                TotalSalesToday = await _orderService.GetDailySalesTotalAsync(DateTime.Today),
                PendingOrdersCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                LowStockItemsCount = await _context.Inventory.CountAsync(i => i.StockLevel <= i.MinimumStockLevel && i.IsActive),
                UpcomingEventsCount = await _context.Events.CountAsync(e => e.EventDate >= DateTime.Today && e.Status == EventStatus.Scheduled),
                RecentOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync(),
                LowStockItems = await _inventoryService.GetLowStockItemsAsync()
            };

            return View(dashboard);
        }

        // Reports Section
        public IActionResult Reports()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SalesReport(DateTime startDate, DateTime endDate)
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var report = await _orderService.GenerateSalesReportAsync(startDate, endDate.AddDays(1).AddSeconds(-1));
            return View("SalesReport", report);
        }

        public async Task<IActionResult> InventoryReport()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var inventory = await _inventoryService.GetAllInventoryAsync();
            var totalValue = await _inventoryService.CalculateTotalInventoryValueAsync();
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync();

            ViewBag.TotalInventoryValue = totalValue;
            ViewBag.LowStockItems = lowStockItems;

            return View(inventory);
        }

        public async Task<IActionResult> EmployeeAttendanceReport()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var attendance = await _reportService.GetEmployeeAttendanceReportAsync(DateTime.Today.AddDays(-30), DateTime.Today);
            return View(attendance);
        }

        // Stock Management
        public async Task<IActionResult> StockManagement()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var inventory = await _inventoryService.GetAllInventoryAsync();
            return View(inventory);
        }

        public async Task<IActionResult> LowStockAlerts()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
            return View(lowStockItems);
        }

        // Order Authorization
        public async Task<IActionResult> PendingOrders()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var pendingOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Inventory)
                .Where(o => o.Status == OrderStatus.Pending)
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            return View(pendingOrders);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrder(int orderId)
        {
            if (!IsManagerAuthorized())
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Approved;
                    order.ApprovedByUserId = GetCurrentUserId();
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Order #{orderId} approved successfully.";
                    return Json(new { success = true, message = "Order approved" });
                }
                return Json(new { success = false, message = "Order not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectOrder(int orderId, string reason)
        {
            if (!IsManagerAuthorized())
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Cancelled;
                    order.CancellationReason = reason;
                    order.ApprovedByUserId = GetCurrentUserId();
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Order #{orderId} rejected.";
                    return Json(new { success = true, message = "Order rejected" });
                }
                return Json(new { success = false, message = "Order not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GRV Authorization
        public async Task<IActionResult> PendingGRVs()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var pendingGRVs = await _context.GoodsReceivedVouchers
                .Include(g => g.PurchaseOrder)
                .Include(g => g.ReceivedBy)
                .Where(g => g.AuthorizedByUserId == null)
                .OrderBy(g => g.ReceivedDate)
                .ToListAsync();

            return View(pendingGRVs);
        }

        [HttpPost]
        public async Task<IActionResult> AuthorizeGRV(int grvId)
        {
            if (!IsManagerAuthorized())
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var grv = await _context.GoodsReceivedVouchers.FindAsync(grvId);
                if (grv != null)
                {
                    grv.AuthorizedByUserId = GetCurrentUserId();
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"GRV #{grvId} authorized successfully.";
                    return Json(new { success = true, message = "GRV authorized" });
                }
                return Json(new { success = false, message = "GRV not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Employee Management
        public async Task<IActionResult> EmployeeManagement()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var employees = await _context.Users
                .Where(u => u.Role == UserRole.Cashier || u.Role == UserRole.EventCoordinator)
                .OrderBy(u => u.Username)
                .ToListAsync();

            return View(employees);
        }

        public async Task<IActionResult> EmployeeSchedules()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var schedules = await _context.EmployeeSchedules
                .Include(es => es.Employee)
                .Where(es => es.ShiftDate >= DateTime.Today)
                .OrderBy(es => es.ShiftDate)
                .ThenBy(es => es.StartTime)
                .ToListAsync();

            return View(schedules);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSchedule()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var employees = await _context.Users
                .Where(u => u.Role == UserRole.Cashier || u.Role == UserRole.EventCoordinator)
                .OrderBy(u => u.Username)
                .ToListAsync();

            ViewBag.Employees = employees;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule(EmployeeSchedule schedule)
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            if (ModelState.IsValid)
            {
                schedule.CreatedByUserId = GetCurrentUserId();
                schedule.CreatedDate = DateTime.Now;

                _context.EmployeeSchedules.Add(schedule);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee schedule created successfully.";
                return RedirectToAction("EmployeeSchedules");
            }

            var employees = await _context.Users
                .Where(u => u.Role == UserRole.Cashier || u.Role == UserRole.EventCoordinator)
                .OrderBy(u => u.Username)
                .ToListAsync();

            ViewBag.Employees = employees;
            return View(schedule);
        }

        // Event Coordination
        public async Task<IActionResult> EventManagement()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.Bookings)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> EventBookings()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            var bookings = await _context.EventBookings
                .Include(eb => eb.Event)
                .Include(eb => eb.Customer)
                .OrderByDescending(eb => eb.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(Event eventItem)
        {
            if (!IsManagerAuthorized())
                return RedirectToAction("AdminLogin", "Auth");

            if (ModelState.IsValid)
            {
                eventItem.CreatedByUserId = GetCurrentUserId();
                eventItem.Status = EventStatus.Scheduled;
                eventItem.CurrentAttendees = 0;

                _context.Events.Add(eventItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Event created successfully.";
                return RedirectToAction("EventManagement");
            }

            return View(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> CancelEvent(int eventId, string reason)
        {
            if (!IsManagerAuthorized())
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var eventItem = await _context.Events.FindAsync(eventId);
                if (eventItem != null)
                {
                    eventItem.Status = EventStatus.Cancelled;
                    // You might want to add a cancellation reason field to the Event model
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Event cancelled successfully.";
                    return Json(new { success = true, message = "Event cancelled" });
                }
                return Json(new { success = false, message = "Event not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Utility Methods
        private bool IsManagerAuthorized()
        {
            var userId = HttpContext.Session.GetInt32("AdminUserId");
            var role = HttpContext.Session.GetString("AdminRole");
            return userId != null && (role == UserRole.Manager.ToString() || role == UserRole.Admin.ToString());
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("AdminUserId") ?? 0;
        }
    }

    public class ManagerDashboardViewModel
    {
        public decimal TotalSalesToday { get; set; }
        public int PendingOrdersCount { get; set; }
        public int LowStockItemsCount { get; set; }
        public int UpcomingEventsCount { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Inventory> LowStockItems { get; set; } = new List<Inventory>();
    }
}