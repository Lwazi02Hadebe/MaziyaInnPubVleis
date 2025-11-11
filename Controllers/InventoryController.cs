// InventoryController.cs
using Microsoft.AspNetCore.Mvc;
using MaziyaInnPubVleis.Models;
using MaziyaInnPubVleis.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using MaziyaInnPubVleis.Data;

namespace MaziyaInnPubVleis.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly ApplicationDbContext _context;

        public InventoryController(IInventoryService inventoryService, ApplicationDbContext context)
        {
            _inventoryService = inventoryService;
            _context = context;
        }

        private async Task PopulateSuppliersViewBag()
        {
            var suppliers = await _context.Suppliers
                .Where(s => s.SupplierId != null)
                .Select(s => new { s.SupplierId, s.Name })
                .ToListAsync();

            ViewBag.Suppliers = new SelectList(suppliers, "SupplierId", "Name");
        }

        public async Task<IActionResult> Index()
        {
            var inventory = await _inventoryService.GetAllInventoryAsync();
            return View(inventory);
        }

        public async Task<IActionResult> Details(int id)
        {
            var inventory = await _inventoryService.GetInventoryWithCalculationsAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            return View(inventory);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateSuppliersViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _inventoryService.AddInventoryAsync(inventory);
                    TempData["SuccessMessage"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating inventory: {ex.Message}");
                    TempData["ErrorMessage"] = $"Error creating product: {ex.Message}";
                }
            }

            await PopulateSuppliersViewBag();
            return View(inventory);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            await PopulateSuppliersViewBag();
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inventory inventory)
        {
            if (id != inventory.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _inventoryService.UpdateInventoryAsync(inventory);
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _inventoryService.GetInventoryByIdAsync(id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating inventory: {ex.Message}");
                    TempData["ErrorMessage"] = $"Error updating product: {ex.Message}";
                }
            }

            await PopulateSuppliersViewBag();
            return View(inventory);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int productId, string confirmationText)
        {
            if (string.IsNullOrEmpty(confirmationText) || confirmationText.ToUpper() != "DELETE")
            {
                TempData["ErrorMessage"] = "Confirmation text 'DELETE' is required to delete the product.";
                return RedirectToAction(nameof(Delete), new { id = productId });
            }

            try
            {
                var result = await _inventoryService.DeleteInventoryAsync(productId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Product deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Product not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting product: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> LowStock()
        {
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
            return View(lowStockItems);
        }

        // New action for testing six-pack sales
        public async Task<IActionResult> TestSixPackSale(int id, int quantity = 1)
        {
            try
            {
                var unitsSold = await _inventoryService.ProcessSixPackSaleAsync(id, quantity);
                TempData["SuccessMessage"] = $"Successfully sold {quantity} six-pack(s) ({unitsSold} units)";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // New action for testing single unit sales
        public async Task<IActionResult> TestSingleUnitSale(int id, int quantity = 1)
        {
            try
            {
                var totalPrice = await _inventoryService.ProcessSingleUnitSaleAsync(id, quantity);
                TempData["SuccessMessage"] = $"Successfully sold {quantity} single unit(s) for R {totalPrice:N2}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}