using MaziyaInnPubVleis.Data;
using MaziyaInnPubVleis.Models;
using Microsoft.EntityFrameworkCore;

namespace MaziyaInnPubVleis.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeAttendanceReport> GetEmployeeAttendanceReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = new EmployeeAttendanceReport
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var employees = await _context.Users
                .Where(u => u.Role == UserRole.Cashier || u.Role == UserRole.EventCoordinator || u.Role == UserRole.Manager)
                .ToListAsync();

            foreach (var employee in employees)
            {
                var attendanceRecords = await _context.EmployeeAttendances
                    .Where(a => a.EmployeeId == employee.UserId && a.WorkDate >= startDate && a.WorkDate <= endDate)
                    .OrderBy(a => a.WorkDate)
                    .ToListAsync();

                var scheduledDays = await _context.EmployeeSchedules
                    .CountAsync(s => s.EmployeeId == employee.UserId && s.ShiftDate >= startDate && s.ShiftDate <= endDate);

                var summary = new EmployeeAttendanceSummary
                {
                    EmployeeId = employee.UserId,
                    EmployeeName = employee.Username,
                    Role = employee.Role.ToString(),
                    DaysWorked = attendanceRecords.Count(a => a.ClockInTime.HasValue),
                    TotalScheduledDays = scheduledDays,
                    TotalHoursWorked = (decimal)(attendanceRecords
                        .Where(a => a.HoursWorked.HasValue)
                        .Sum(a => a.HoursWorked.Value.TotalHours)),
                    Absences = scheduledDays - attendanceRecords.Count(a => a.ClockInTime.HasValue),
                    LateArrivals = attendanceRecords.Count(a => a.Status == "Late"),
                    AttendanceRecords = attendanceRecords
                };

                report.EmployeeSummaries.Add(summary);
            }

            report.AverageHoursWorked = report.EmployeeSummaries.Any()
                ? report.EmployeeSummaries.Average(s => s.TotalHoursWorked)
                : 0;
            report.TotalAbsences = report.EmployeeSummaries.Sum(s => s.Absences);
            report.TotalLateArrivals = report.EmployeeSummaries.Sum(s => s.LateArrivals);

            return report;
        }

        public async Task<FinancialReport> GetFinancialReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Inventory)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed)
                .ToListAsync();

            var report = new FinancialReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                TotalCosts = orders.Sum(o => o.OrderItems.Sum(oi => oi.Inventory.CostPrice * oi.Quantity)),
                GrossProfit = orders.Sum(o => o.GrossProfit)
            };

            report.NetProfit = report.GrossProfit; // Simplified - in reality you'd subtract other costs

            // Calculate revenue by category (simplified)
            var productCategories = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Inventory.IsSixPack ? "Beverages" : "Food")
                .Select(g => new RevenueByCategory
                {
                    Category = g.Key,
                    Amount = g.Sum(oi => oi.TotalPrice),
                    Percentage = report.TotalRevenue > 0 ? (g.Sum(oi => oi.TotalPrice) / report.TotalRevenue) * 100 : 0
                })
                .ToList();

            report.RevenueByCategory = productCategories;

            // Calculate monthly summaries
            var monthlyData = orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlySummary
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(o => o.TotalAmount),
                    Profit = g.Sum(o => o.GrossProfit)
                })
                .OrderBy(m => m.Month)
                .ToList();

            report.MonthlySummaries = monthlyData;

            return report;
        }

        public async Task<List<EmployeeSchedule>> GetEmployeeSchedulesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.EmployeeSchedules
                .Include(es => es.Employee)
                .Where(es => es.ShiftDate >= startDate && es.ShiftDate <= endDate)
                .OrderBy(es => es.ShiftDate)
                .ThenBy(es => es.StartTime)
                .ToListAsync();
        }

        public async Task<bool> RecordClockInAsync(int employeeId)
        {
            var today = DateTime.Today;
            var existingAttendance = await _context.EmployeeAttendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.WorkDate == today);

            if (existingAttendance != null)
            {
                // Already clocked in today
                return false;
            }

            var attendance = new EmployeeAttendance
            {
                EmployeeId = employeeId,
                WorkDate = today,
                ClockInTime = DateTime.Now,
                Status = "Present"
            };

            // Check if late (assuming morning shift starts at 8 AM)
            var schedule = await _context.EmployeeSchedules
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.ShiftDate == today);

            if (schedule != null && DateTime.Now.TimeOfDay > schedule.StartTime.Add(TimeSpan.FromMinutes(15)))
            {
                attendance.Status = "Late";
            }

            _context.EmployeeAttendances.Add(attendance);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RecordClockOutAsync(int employeeId)
        {
            var today = DateTime.Today;
            var attendance = await _context.EmployeeAttendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.WorkDate == today);

            if (attendance == null || attendance.ClockOutTime.HasValue)
            {
                return false;
            }

            attendance.ClockOutTime = DateTime.Now;

            // Check if left early (assuming minimum 8-hour shift)
            var schedule = await _context.EmployeeSchedules
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.ShiftDate == today);

            if (schedule != null && attendance.HoursWorked.HasValue &&
                attendance.HoursWorked.Value.TotalHours < 8)
            {
                attendance.Status = "Left Early";
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}