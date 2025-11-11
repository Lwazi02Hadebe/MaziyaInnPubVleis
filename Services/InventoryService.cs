// InventoryService.cs
using MaziyaInnPubVleis.Data;
using MaziyaInnPubVleis.Models;
using Microsoft.EntityFrameworkCore;

namespace MaziyaInnPubVleis.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISixPackService _sixPackService;

        public InventoryService(ApplicationDbContext context, ISixPackService sixPackService)
        {
            _context = context;
            _sixPackService = sixPackService;
        }

        public async Task<List<Inventory>> GetAllInventoryAsync()
        {
            return await _context.Inventory
                .Include(i => i.Supplier)
                .Where(i => i.IsActive)
                .OrderBy(i => i.ProductName)
                .ToListAsync();
        }

        public async Task<List<Inventory>> GetInventoryWithSixPackCalculationsAsync()
        {
            var inventory = await GetAllInventoryAsync();

            // Calculate unit metrics for each inventory item
            foreach (var item in inventory)
            {
                var metrics = _sixPackService.CalculateUnitMetrics(item);
                item.SingleUnitPrice = metrics.unitPrice;
                item.SingleUnitCost = metrics.unitCost;
                item.SingleUnitProfit = metrics.unitProfit;
            }

            return inventory;
        }

        public async Task<Inventory> GetInventoryByIdAsync(int id)
        {
            return await _context.Inventory
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(i => i.ProductId == id && i.IsActive);
        }

        public async Task<Inventory> GetInventoryWithCalculationsAsync(int id)
        {
            var inventory = await GetInventoryByIdAsync(id);
            if (inventory != null)
            {
                var metrics = _sixPackService.CalculateUnitMetrics(inventory);
                inventory.SingleUnitPrice = metrics.unitPrice;
                inventory.SingleUnitCost = metrics.unitCost;
                inventory.SingleUnitProfit = metrics.unitProfit;
            }
            return inventory;
        }

        public async Task<Inventory> AddInventoryAsync(Inventory inventory)
        {
            // Validate supplier exists if provided
            if (inventory.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers
                    .AnyAsync(s => s.SupplierId == inventory.SupplierId.Value);

                if (!supplierExists)
                {
                    throw new InvalidOperationException("Selected supplier does not exist.");
                }
            }

            // Auto-detect alcohol products and set as six-pack
            if (_sixPackService.IsAlcoholProduct(inventory) && !inventory.IsSixPack)
            {
                inventory.IsSixPack = true;
                inventory.SixPackQuantity = 6;

                // If prices were entered as single units, convert to six-pack prices
                if (inventory.UnitPrice < 50) // Assuming single unit beer price would be less than 50
                {
                    inventory.UnitPrice = _sixPackService.CalculateSixPackPriceFromUnit(inventory.UnitPrice);
                    inventory.CostPrice = _sixPackService.CalculateSixPackCostFromUnit(inventory.CostPrice);
                }
            }

            // Ensure stock level is not negative
            if (inventory.StockLevel < 0)
            {
                inventory.StockLevel = 0;
            }

            // Ensure minimum stock level is reasonable
            if (inventory.MinimumStockLevel < 0)
            {
                inventory.MinimumStockLevel = 0;
            }

            _context.Inventory.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<Inventory> UpdateInventoryAsync(Inventory inventory)
        {
            // Validate supplier exists if provided
            if (inventory.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers
                    .AnyAsync(s => s.SupplierId == inventory.SupplierId.Value);

                if (!supplierExists)
                {
                    throw new InvalidOperationException("Selected supplier does not exist.");
                }
            }

            // Auto-detect alcohol products and set as six-pack
            if (_sixPackService.IsAlcoholProduct(inventory) && !inventory.IsSixPack)
            {
                inventory.IsSixPack = true;
                inventory.SixPackQuantity = 6;
            }

            // Ensure stock level is not negative
            if (inventory.StockLevel < 0)
            {
                inventory.StockLevel = 0;
            }

            // Ensure minimum stock level is reasonable
            if (inventory.MinimumStockLevel < 0)
            {
                inventory.MinimumStockLevel = 0;
            }

            _context.Inventory.Update(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            var inventory = await _context.Inventory.FindAsync(id);
            if (inventory != null)
            {
                // Soft delete by setting IsActive to false
                inventory.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateStockLevelAsync(int productId, int quantityChange)
        {
            var inventory = await _context.Inventory.FindAsync(productId);
            if (inventory != null)
            {
                var newStockLevel = inventory.StockLevel + quantityChange;
                if (newStockLevel < 0)
                {
                    throw new InvalidOperationException($"Cannot reduce stock below 0. Current stock: {inventory.StockLevel}, attempted reduction: {quantityChange}");
                }

                inventory.StockLevel = newStockLevel;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<Inventory>> GetLowStockItemsAsync()
        {
            return await _context.Inventory
                .Where(i => i.StockLevel <= i.MinimumStockLevel && i.IsActive)
                .Include(i => i.Supplier)
                .OrderBy(i => i.StockLevel)
                .ToListAsync();
        }

        public async Task<decimal> CalculateTotalInventoryValueAsync()
        {
            return await _context.Inventory
                .Where(i => i.IsActive)
                .SumAsync(i => i.StockLevel * i.CostPrice);
        }

        public async Task<int> ProcessSixPackSaleAsync(int productId, int sixPackQuantity)
        {
            if (sixPackQuantity <= 0)
            {
                throw new InvalidOperationException("Quantity must be greater than 0.");
            }

            var inventory = await _context.Inventory.FindAsync(productId);
            if (inventory == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            if (!inventory.IsSixPack)
            {
                throw new InvalidOperationException("Product is not configured as six-pack.");
            }

            int unitsToDeduct = sixPackQuantity * inventory.SixPackQuantity;
            if (inventory.StockLevel < unitsToDeduct)
            {
                throw new InvalidOperationException($"Insufficient stock for six-pack sale. Available: {inventory.StockLevel}, Required: {unitsToDeduct}");
            }

            inventory.StockLevel -= unitsToDeduct;
            await _context.SaveChangesAsync();
            return unitsToDeduct;
        }

        public async Task<decimal> ProcessSingleUnitSaleAsync(int productId, int singleUnitQuantity)
        {
            if (singleUnitQuantity <= 0)
            {
                throw new InvalidOperationException("Quantity must be greater than 0.");
            }

            var inventory = await _context.Inventory.FindAsync(productId);
            if (inventory == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            if (inventory.StockLevel < singleUnitQuantity)
            {
                throw new InvalidOperationException($"Insufficient stock. Available: {inventory.StockLevel}, Required: {singleUnitQuantity}");
            }

            decimal totalPrice;
            if (inventory.IsSixPack)
            {
                // For six-pack items, calculate price at single unit rate
                var metrics = _sixPackService.CalculateUnitMetrics(inventory);
                totalPrice = singleUnitQuantity * metrics.unitPrice;
            }
            else
            {
                // For non-six-pack items, use normal pricing
                totalPrice = singleUnitQuantity * inventory.UnitPrice;
            }

            inventory.StockLevel -= singleUnitQuantity;
            await _context.SaveChangesAsync();
            return totalPrice;
        }

        public async Task<bool> ReserveStockForEventAsync(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                throw new InvalidOperationException("Quantity must be greater than 0.");
            }

            var inventory = await _context.Inventory.FindAsync(productId);
            if (inventory != null && inventory.StockLevel >= quantity)
            {
                inventory.StockLevel -= quantity;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ReturnStockAsync(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                throw new InvalidOperationException("Quantity must be greater than 0.");
            }

            var inventory = await _context.Inventory.FindAsync(productId);
            if (inventory != null)
            {
                inventory.StockLevel += quantity;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}