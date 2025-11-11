// SixPackService.cs
using MaziyaInnPubVleis.Models;

namespace MaziyaInnPubVleis.Services
{
    public class SixPackService : ISixPackService
    {
        private const int DEFAULT_SIX_PACK_QUANTITY = 6;
        private readonly List<string> _alcoholKeywords = new List<string>
        {
            "beer", "lager", "ale", "stout", "wine", "whisky", "whiskey", "vodka",
            "gin", "rum", "tequila", "brandy", "cider", "champagne", "cocktail",
            "brew", "draught", "bottle", "can", "alcohol", "alcoholic"
        };

        public decimal CalculateUnitPriceFromSixPack(decimal sixPackPrice, int sixPackQuantity = 6)
        {
            if (sixPackQuantity <= 0) sixPackQuantity = DEFAULT_SIX_PACK_QUANTITY;
            return Math.Round(sixPackPrice / sixPackQuantity, 2);
        }

        public decimal CalculateCostPriceFromSixPack(decimal sixPackCostPrice, int sixPackQuantity = 6)
        {
            if (sixPackQuantity <= 0) sixPackQuantity = DEFAULT_SIX_PACK_QUANTITY;
            return Math.Round(sixPackCostPrice / sixPackQuantity, 2);
        }

        public decimal CalculateSixPackPriceFromUnit(decimal unitPrice, int sixPackQuantity = 6)
        {
            if (sixPackQuantity <= 0) sixPackQuantity = DEFAULT_SIX_PACK_QUANTITY;
            return Math.Round(unitPrice * sixPackQuantity, 2);
        }

        public decimal CalculateSixPackCostFromUnit(decimal unitCost, int sixPackQuantity = 6)
        {
            if (sixPackQuantity <= 0) sixPackQuantity = DEFAULT_SIX_PACK_QUANTITY;
            return Math.Round(unitCost * sixPackQuantity, 2);
        }

        public int GetUnitsInSixPack(int sixPackQuantity = 6)
        {
            return sixPackQuantity > 0 ? sixPackQuantity : DEFAULT_SIX_PACK_QUANTITY;
        }

        public decimal CalculateProfitForSingleUnit(decimal sixPackCostPrice, decimal sixPackSellingPrice, int sixPackQuantity = 6)
        {
            var unitCost = CalculateCostPriceFromSixPack(sixPackCostPrice, sixPackQuantity);
            var unitPrice = CalculateUnitPriceFromSixPack(sixPackSellingPrice, sixPackQuantity);
            return Math.Round(unitPrice - unitCost, 2);
        }

        public SixPackCalculationResult CalculateSixPackBreakdown(Inventory inventory)
        {
            if (!inventory.IsSixPack)
            {
                return new SixPackCalculationResult
                {
                    UnitPrice = inventory.UnitPrice,
                    UnitCostPrice = inventory.CostPrice,
                    UnitProfit = inventory.UnitPrice - inventory.CostPrice,
                    SixPackPrice = inventory.UnitPrice * DEFAULT_SIX_PACK_QUANTITY,
                    SixPackCost = inventory.CostPrice * DEFAULT_SIX_PACK_QUANTITY,
                    SixPackProfit = (inventory.UnitPrice - inventory.CostPrice) * DEFAULT_SIX_PACK_QUANTITY,
                    UnitsPerPack = DEFAULT_SIX_PACK_QUANTITY
                };
            }

            var unitPrice = CalculateUnitPriceFromSixPack(inventory.UnitPrice, inventory.SixPackQuantity);
            var unitCost = CalculateCostPriceFromSixPack(inventory.CostPrice, inventory.SixPackQuantity);
            var unitProfit = unitPrice - unitCost;

            return new SixPackCalculationResult
            {
                UnitPrice = unitPrice,
                UnitCostPrice = unitCost,
                UnitProfit = unitProfit,
                SixPackPrice = inventory.UnitPrice,
                SixPackCost = inventory.CostPrice,
                SixPackProfit = inventory.UnitPrice - inventory.CostPrice,
                UnitsPerPack = GetUnitsInSixPack(inventory.SixPackQuantity)
            };
        }

        public bool IsAlcoholProduct(Inventory inventory)
        {
            if (inventory == null) return false;

            var productName = inventory.ProductName?.ToLower() ?? "";
            var description = inventory.Description?.ToLower() ?? "";

            return _alcoholKeywords.Any(keyword =>
                productName.Contains(keyword) || description.Contains(keyword));
        }

        public int CalculateUnitsToDeduct(int quantity, bool isSixPack, int sixPackQuantity = 6)
        {
            if (!isSixPack) return quantity;
            return quantity * (sixPackQuantity > 0 ? sixPackQuantity : DEFAULT_SIX_PACK_QUANTITY);
        }

        public (decimal unitPrice, decimal unitCost, decimal unitProfit) CalculateUnitMetrics(Inventory inventory)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            decimal unitPrice, unitCost, unitProfit;

            if (inventory.IsSixPack)
            {
                unitPrice = CalculateUnitPriceFromSixPack(inventory.UnitPrice, inventory.SixPackQuantity);
                unitCost = CalculateCostPriceFromSixPack(inventory.CostPrice, inventory.SixPackQuantity);
                unitProfit = CalculateProfitForSingleUnit(inventory.CostPrice, inventory.UnitPrice, inventory.SixPackQuantity);
            }
            else
            {
                unitPrice = inventory.UnitPrice;
                unitCost = inventory.CostPrice;
                unitProfit = inventory.UnitPrice - inventory.CostPrice;
            }

            return (unitPrice, unitCost, unitProfit);
        }
    }
}