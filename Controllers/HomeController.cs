using Microsoft.AspNetCore.Mvc;
using MaziyaInnPubVleis.Services;
using MaziyaInnPubVleis.Models;

namespace MaziyaInnPubVleis.Controllers
{
    public class HomeController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IOrderService _orderService;

        public HomeController(IInventoryService inventoryService, IOrderService orderService)
        {
            _inventoryService = inventoryService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
            var totalInventoryValue = await _inventoryService.CalculateTotalInventoryValueAsync();

            ViewBag.LowStockItems = lowStockItems;
            ViewBag.TotalInventoryValue = totalInventoryValue;

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}