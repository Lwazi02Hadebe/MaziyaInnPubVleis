
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MaziyaInnPubVleis.Services;

namespace MaziyaInnPubVleis.Models
{
    public class Inventory
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue)]
        public int StockLevel { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MinimumStockLevel { get; set; } = 10;

        public bool IsSixPack { get; set; } = false;

        [Range(1, int.MaxValue)]
        public int SixPackQuantity { get; set; } = 6;

        public bool IsActive { get; set; } = true;

        // Foreign key - make it nullable
        public int? SupplierId { get; set; }

        // Navigation property
        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
        public ICollection<EventStock> EventStocks { get; set; } = new List<EventStock>();

        // Calculated properties - These will be set by the service layer
        [NotMapped]
        public decimal ProfitPerUnit => UnitPrice - CostPrice;

        [NotMapped]
        public decimal ProfitMargin => UnitPrice > 0 ? (ProfitPerUnit / UnitPrice) * 100 : 0;

        // These properties will be calculated by the SixPackService
        [NotMapped]
        public decimal SingleUnitPrice { get; set; }

        [NotMapped]
        public decimal SingleUnitCost { get; set; }

        [NotMapped]
        public decimal SingleUnitProfit { get; set; }

        [NotMapped]
        public int UnitsPerPack => IsSixPack ? (SixPackQuantity > 0 ? SixPackQuantity : 6) : 1;

        [NotMapped]
        public decimal SixPackProfit => IsSixPack ? ProfitPerUnit : ProfitPerUnit * 6;

        [NotMapped]
        public string DisplayPrice => IsSixPack ?
            $"R{SingleUnitPrice:N2} per unit (R{UnitPrice:N2} per {SixPackQuantity}-pack)" :
            $"R{UnitPrice:N2}";

        [NotMapped]
        public string StockStatus
        {
            get
            {
                if (StockLevel == 0)
                    return "Out of Stock";
                if (StockLevel <= MinimumStockLevel)
                    return "Low Stock";
                return "In Stock";
            }
        }

        [NotMapped]
        public string StockStatusClass
        {
            get
            {
                if (StockLevel == 0)
                    return "out-of-stock";
                if (StockLevel <= MinimumStockLevel)
                    return "low-stock";
                return "in-stock";
            }
        }

        // Method to calculate unit metrics using service (for use in controllers/views)
        public void CalculateUnitMetrics(ISixPackService sixPackService)
        {
            if (sixPackService == null) return;

            if (IsSixPack)
            {
                SingleUnitPrice = sixPackService.CalculateUnitPriceFromSixPack(UnitPrice, SixPackQuantity);
                SingleUnitCost = sixPackService.CalculateCostPriceFromSixPack(CostPrice, SixPackQuantity);
                SingleUnitProfit = sixPackService.CalculateProfitForSingleUnit(CostPrice, UnitPrice, SixPackQuantity);
            }
            else
            {
                SingleUnitPrice = UnitPrice;
                SingleUnitCost = CostPrice;
                SingleUnitProfit = ProfitPerUnit;
            }
        }

        // Method to get the appropriate price based on context
        public decimal GetPriceForQuantity(int quantity, bool forceSingleUnit = false)
        {
            if (IsSixPack && !forceSingleUnit)
            {
                // For six-pack items, calculate based on single unit pricing
                return quantity * SingleUnitPrice;
            }
            else
            {
                // For single items or forced single unit pricing
                return quantity * UnitPrice;
            }
        }

        // Method to calculate actual units needed for stock deduction
        public int CalculateUnitsToDeduct(int quantity)
        {
            if (IsSixPack)
            {
                return quantity * SixPackQuantity;
            }
            return quantity;
        }

        // Method to check if stock is sufficient for a given quantity
        public bool IsStockSufficient(int quantity)
        {
            int unitsNeeded = CalculateUnitsToDeduct(quantity);
            return StockLevel >= unitsNeeded;
        }

        // Method to get available quantity (in terms of packs or units)
        public int GetAvailableQuantity(bool inPacks = false)
        {
            if (IsSixPack && inPacks)
            {
                return StockLevel / SixPackQuantity;
            }
            return StockLevel;
        }

        // Method to update stock level safely
        public bool UpdateStockLevel(int quantityChange)
        {
            var newStockLevel = StockLevel + quantityChange;
            if (newStockLevel < 0) return false;

            StockLevel = newStockLevel;
            return true;
        }

        // Override ToString for debugging and display purposes
        public override string ToString()
        {
            return $"{ProductName} - {ProductId} (Stock: {StockLevel}, SixPack: {IsSixPack})";
        }

        // Static method to create a new inventory item with six-pack auto-detection
        public static Inventory Create(string productName, string description, int stockLevel,
            decimal unitPrice, decimal costPrice, int minimumStockLevel = 10,
            int? supplierId = null, ISixPackService? sixPackService = null)
        {
            var inventory = new Inventory
            {
                ProductName = productName,
                Description = description,
                StockLevel = stockLevel,
                UnitPrice = unitPrice,
                CostPrice = costPrice,
                MinimumStockLevel = minimumStockLevel,
                SupplierId = supplierId,
                IsActive = true
            };

            // Auto-detect alcohol products if service is provided
            if (sixPackService != null && sixPackService.IsAlcoholProduct(inventory))
            {
                inventory.IsSixPack = true;
                inventory.SixPackQuantity = 6;

                // Convert prices to six-pack prices if they look like single unit prices
                if (unitPrice < 50) // Assuming single unit beer price would be less than 50
                {
                    inventory.UnitPrice = sixPackService.CalculateSixPackPriceFromUnit(unitPrice);
                    inventory.CostPrice = sixPackService.CalculateSixPackCostFromUnit(costPrice);
                }
            }

            // Calculate initial unit metrics
            if (sixPackService != null)
            {
                inventory.CalculateUnitMetrics(sixPackService);
            }

            return inventory;
        }
    }

    // Extension methods for Inventory
    public static class InventoryExtensions
    {
        public static decimal CalculateTotalValue(this Inventory inventory)
        {
            return inventory.StockLevel * inventory.CostPrice;
        }

        public static decimal CalculatePotentialRevenue(this Inventory inventory)
        {
            if (inventory.IsSixPack)
            {
                return inventory.StockLevel * inventory.SingleUnitPrice;
            }
            return inventory.StockLevel * inventory.UnitPrice;
        }

        public static decimal CalculateTotalProfitPotential(this Inventory inventory)
        {
            if (inventory.IsSixPack)
            {
                return inventory.StockLevel * inventory.SingleUnitProfit;
            }
            return inventory.StockLevel * inventory.ProfitPerUnit;
        }

        public static string GetPackDescription(this Inventory inventory)
        {
            return inventory.IsSixPack ?
                $"{inventory.SixPackQuantity}-pack" : "Single unit";
        }

        public static bool IsAlcoholProduct(this Inventory inventory, ISixPackService sixPackService)
        {
            return sixPackService.IsAlcoholProduct(inventory);
        }

        public static SixPackCalculationResult GetSixPackBreakdown(this Inventory inventory, ISixPackService sixPackService)
        {
            return sixPackService.CalculateSixPackBreakdown(inventory);
        }
    }
}
