// OrderService.cs
using MaziyaInnPubVleis.Data;
using MaziyaInnPubVleis.Models;
using Microsoft.EntityFrameworkCore;

namespace MaziyaInnPubVleis.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly ISixPackService _sixPackService;

        public OrderService(ApplicationDbContext context, IInventoryService inventoryService, ISixPackService sixPackService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _sixPackService = sixPackService;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.ProcessedBy)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Inventory)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.ProcessedBy)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Inventory)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<Order> CreateOrderAsync(Order order, List<OrderItem> orderItems)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Calculate order totals with six-pack considerations
                var summary = await CalculateOrderSummaryAsync(orderItems);
                order.TotalAmount = summary.TotalAmount;
                order.VATAmount = summary.VATAmount;
                order.GrossProfit = summary.GrossProfit;
                order.OrderDate = DateTime.UtcNow;
                order.Status = OrderStatus.Completed;

                // Add order to context
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order items and update inventory
                foreach (var item in orderItems)
                {
                    item.OrderId = order.OrderId;
                    _context.OrderItems.Add(item);

                    // Update inventory stock levels
                    var inventory = await _inventoryService.GetInventoryByIdAsync(item.ProductId);
                    if (inventory != null)
                    {
                        int unitsToDeduct = _sixPackService.CalculateUnitsToDeduct(
                            item.Quantity,
                            inventory.IsSixPack,
                            inventory.SixPackQuantity);

                        await _inventoryService.UpdateStockLevelAsync(item.ProductId, -unitsToDeduct);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetOrderByIdAsync(order.OrderId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Order> ProcessOrderWithSixPackItemsAsync(Order order, List<OrderItem> orderItems)
        {
            // This method specifically handles six-pack conversions
            foreach (var item in orderItems)
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(item.ProductId);
                if (inventory != null && inventory.IsSixPack)
                {
                    // Calculate the actual unit price based on six-pack configuration
                    var metrics = _sixPackService.CalculateUnitMetrics(inventory);
                    item.UnitPrice = metrics.unitPrice;
                    item.TotalPrice = item.Quantity * metrics.unitPrice;
                }
            }

            return await CreateOrderAsync(order, orderItems);
        }

        public async Task<decimal> CalculateOrderTotalWithSixPacksAsync(List<OrderItem> orderItems)
        {
            decimal total = 0;

            foreach (var item in orderItems)
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(item.ProductId);
                if (inventory != null)
                {
                    if (inventory.IsSixPack)
                    {
                        var metrics = _sixPackService.CalculateUnitMetrics(inventory);
                        total += item.Quantity * metrics.unitPrice;
                    }
                    else
                    {
                        total += item.Quantity * inventory.UnitPrice;
                    }
                }
            }

            return total;
        }

        public async Task<OrderSummary> CalculateOrderSummaryAsync(List<OrderItem> orderItems)
        {
            var summary = new OrderSummary();
            decimal subtotal = 0;
            decimal totalCost = 0;

            foreach (var item in orderItems)
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(item.ProductId);
                if (inventory != null)
                {
                    decimal itemPrice, itemCost;
                    int actualUnits;

                    if (inventory.IsSixPack)
                    {
                        var metrics = _sixPackService.CalculateUnitMetrics(inventory);
                        itemPrice = metrics.unitPrice;
                        itemCost = metrics.unitCost;
                        actualUnits = item.Quantity * inventory.SixPackQuantity;
                    }
                    else
                    {
                        itemPrice = inventory.UnitPrice;
                        itemCost = inventory.CostPrice;
                        actualUnits = item.Quantity;
                    }

                    decimal itemTotal = item.Quantity * itemPrice;
                    decimal itemTotalCost = item.Quantity * itemCost;

                    subtotal += itemTotal;
                    totalCost += itemTotalCost;

                    summary.Items.Add(new OrderItemSummary
                    {
                        ProductId = inventory.ProductId,
                        ProductName = inventory.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = itemPrice,
                        TotalPrice = itemTotal,
                        IsSixPack = inventory.IsSixPack,
                        SixPackQuantity = inventory.SixPackQuantity,
                        ActualUnits = actualUnits
                    });
                }
            }

            // Calculate VAT (15% in South Africa)
            summary.Subtotal = Math.Round(subtotal, 2);
            summary.VATAmount = Math.Round(subtotal * 0.15m, 2);
            summary.TotalAmount = Math.Round(subtotal + summary.VATAmount, 2);
            summary.TotalCost = Math.Round(totalCost, 2);
            summary.GrossProfit = Math.Round(subtotal - totalCost, 2);

            return summary;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Inventory)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<SalesReport> GenerateSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await GetOrdersByDateRangeAsync(startDate, endDate);
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();

            var report = new SalesReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalOrders = completedOrders.Count,
                TotalSales = completedOrders.Sum(o => o.TotalAmount),
                TotalVAT = completedOrders.Sum(o => o.VATAmount),
                TotalGrossProfit = completedOrders.Sum(o => o.GrossProfit),
                Orders = completedOrders
            };

            report.AverageProfitMargin = report.TotalSales > 0 ?
                (report.TotalGrossProfit / report.TotalSales) * 100 : 0;

            // Calculate top selling products
            var productSales = completedOrders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new { oi.ProductId, oi.Inventory.ProductName, oi.Inventory.IsSixPack, oi.Inventory.SixPackQuantity })
                .Select(g => new ProductSales
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice),
                    IsSixPack = g.Key.IsSixPack,
                    ActualUnitsSold = g.Key.IsSixPack ?
                        g.Sum(oi => oi.Quantity * g.Key.SixPackQuantity) :
                        g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(ps => ps.TotalRevenue)
                .Take(10)
                .ToList();

            report.TopSellingProducts = productSales;

            return report;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Inventory)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null) return false;

                // Return stock to inventory
                foreach (var item in order.OrderItems)
                {
                    var inventory = item.Inventory;
                    if (inventory != null)
                    {
                        int unitsToReturn = _sixPackService.CalculateUnitsToDeduct(
                            item.Quantity,
                            inventory.IsSixPack,
                            inventory.SixPackQuantity);

                        await _inventoryService.UpdateStockLevelAsync(item.ProductId, unitsToReturn);
                    }
                }

                // Update order status
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<decimal> GetDailySalesTotalAsync(DateTime date)
        {
            return await _context.Orders
                .Where(o => o.OrderDate.Date == date.Date && o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);
        }
    }
}