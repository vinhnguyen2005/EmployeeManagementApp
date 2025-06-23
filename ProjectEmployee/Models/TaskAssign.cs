using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectEmployee.Models
{
    public partial class TaskAssign
    {
        public int TaskId { get; set; }

        public int EmployeeId { get; set; }

        public string TaskDescription { get; set; }

        public DateTime Deadline { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public decimal? PerformanceScore { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }
    }
}