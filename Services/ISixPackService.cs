// ISixPackService.cs
using MaziyaInnPubVleis.Models;

namespace MaziyaInnPubVleis.Services
{
    public interface ISixPackService
    {
        decimal CalculateUnitPriceFromSixPack(decimal sixPackPrice, int sixPackQuantity = 6);
        decimal CalculateCostPriceFromSixPack(decimal sixPackCostPrice, int sixPackQuantity = 6);
        int GetUnitsInSixPack(int sixPackQuantity = 6);
        decimal CalculateProfitForSingleUnit(decimal sixPackCostPrice, decimal sixPackSellingPrice, int sixPackQuantity = 6);
        SixPackCalculationResult CalculateSixPackBreakdown(Inventory inventory);
        bool IsAlcoholProduct(Inventory inventory);
        int CalculateUnitsToDeduct(int quantity, bool isSixPack, int sixPackQuantity = 6);

        // New methods for comprehensive six-pack calculations
        decimal CalculateSixPackPriceFromUnit(decimal unitPrice, int sixPackQuantity = 6);
        decimal CalculateSixPackCostFromUnit(decimal unitCost, int sixPackQuantity = 6);
        (decimal unitPrice, decimal unitCost, decimal unitProfit) CalculateUnitMetrics(Inventory inventory);
    }

    public class SixPackCalculationResult
    {
        public decimal UnitPrice { get; set; }
        public decimal UnitCostPrice { get; set; }
        public decimal UnitProfit { get; set; }
        public decimal SixPackPrice { get; set; }
        public decimal SixPackCost { get; set; }
        public decimal SixPackProfit { get; set; }
        public int UnitsPerPack { get; set; }
    }
}