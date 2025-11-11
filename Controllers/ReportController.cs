using Microsoft.AspNetCore.Mvc;
using MaziyaInnPubVleis.Services;

namespace MaziyaInnPubVleis.Controllers
{
    public class ReportController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IInventoryService _inventoryService;

        public ReportController(IOrderService orderService, IInventoryService inventoryService)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
        }

        public IActionResult Sales()
        {
            // Default to current month
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = DateTime.Today;

            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SalesReport(DateTime startDate, DateTime endDate)
        {
            var report = await _orderService.GenerateSalesReportAsync(startDate, endDate.AddDays(1).AddSeconds(-1));
            return View("SalesReport", report);
        }

        public async Task<IActionResult> DailySales()
        {
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(1).AddSeconds(-1);

            var report = await _orderService.GenerateSalesReportAsync(startDate, endDate);
            return View(report);
        }

        public async Task<IActionResult> Inventory()
        {
            var inventory = await _inventoryService.GetAllInventoryAsync();
            var totalValue = await _inventoryService.CalculateTotalInventoryValueAsync();
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync();

            ViewBag.TotalInventoryValue = totalValue;
            ViewBag.LowStockItems = lowStockItems;

            return View(inventory);
        }
    }
}