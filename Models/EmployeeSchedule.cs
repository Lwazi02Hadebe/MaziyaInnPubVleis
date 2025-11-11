using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaziyaInnPubVleis.Models
{
    public class EmployeeSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime ShiftDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [StringLength(50)]
        public string ShiftType { get; set; } // "Morning", "Evening", "Night", etc.

        [StringLength(200)]
        public string Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public User Employee { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User CreatedBy { get; set; }
    }

    public class EmployeeAttendance
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime WorkDate { get; set; }

        public DateTime? ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }

        public TimeSpan? HoursWorked => ClockInTime.HasValue && ClockOutTime.HasValue
            ? ClockOutTime.Value - ClockInTime.Value
            : null;

        [StringLength(50)]
        public string Status { get; set; } // "Present", "Absent", "Late", "Left Early"

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public User Employee { get; set; }
    }
}