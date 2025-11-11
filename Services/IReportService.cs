using MaziyaInnPubVleis.Models;

namespace MaziyaInnPubVleis.Services
{
    public interface IReportService
    {
        Task<EmployeeAttendanceReport> GetEmployeeAttendanceReportAsync(DateTime startDate, DateTime endDate);
        Task<FinancialReport> GetFinancialReportAsync(DateTime startDate, DateTime endDate);
        Task<List<EmployeeSchedule>> GetEmployeeSchedulesAsync(DateTime startDate, DateTime endDate);
        Task<bool> RecordClockInAsync(int employeeId);
        Task<bool> RecordClockOutAsync(int employeeId);
    }

    public class EmployeeAttendanceReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<EmployeeAttendanceSummary> EmployeeSummaries { get; set; } = new List<EmployeeAttendanceSummary>();
        public decimal AverageHoursWorked { get; set; }
        public int TotalAbsences { get; set; }
        public int TotalLateArrivals { get; set; }
    }

    public class EmployeeAttendanceSummary
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int DaysWorked { get; set; }
        public int TotalScheduledDays { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public int Absences { get; set; }
        public int LateArrivals { get; set; }
        public List<EmployeeAttendance> AttendanceRecords { get; set; } = new List<EmployeeAttendance>();
    }

    public class FinancialReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public List<RevenueByCategory> RevenueByCategory { get; set; } = new List<RevenueByCategory>();
        public List<MonthlySummary> MonthlySummaries { get; set; } = new List<MonthlySummary>();
    }

    public class RevenueByCategory
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class MonthlySummary
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }
}