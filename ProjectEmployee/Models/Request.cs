using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectEmployee.Models
{
    public partial class Request
    {
        public int RequestId { get; set; }
        public int? OriginatorId { get; set; }
        public int EmployeeId { get; set; }
        public int ManagerId { get; set; }
        public string RequestType { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal? RaiseAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        [ForeignKey("ManagerId")]
        public virtual Employee Manager { get; set; } = null!;

        [ForeignKey("OriginatorId")]
        public virtual Employee? Originator { get; set; }
    }
}
