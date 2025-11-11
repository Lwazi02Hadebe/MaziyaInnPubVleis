// IInventoryService.cs
using MaziyaInnPubVleis.Models;

namespace MaziyaInnPubVleis.Services
{
    public interface IInventoryService
    {
        Task<List<Inventory>> GetAllInventoryAsync();
        Task<Inventory> GetInventoryByIdAsync(int id);
        Task<Inventory> AddInventoryAsync(Inventory inventory);
        Task<Inventory> UpdateInventoryAsync(Inventory inventory);
        Task<bool> DeleteInventoryAsync(int id);
        Task<bool> UpdateStockLevelAsync(int productId, int quantityChange);
        Task<List<Inventory>> GetLowStockItemsAsync();
        Task<decimal> CalculateTotalInventoryValueAsync();
        Task<int> ProcessSixPackSaleAsync(int productId, int sixPackQuantity);
        Task<bool> ReserveStockForEventAsync(int productId, int quantity);
        Task<bool> ReturnStockAsync(int productId, int quantity);

        // New methods for six-pack integration
        Task<decimal> ProcessSingleUnitSaleAsync(int productId, int singleUnitQuantity);
        Task<Inventory> GetInventoryWithCalculationsAsync(int id);
        Task<List<Inventory>> GetInventoryWithSixPackCalculationsAsync();
    }
}