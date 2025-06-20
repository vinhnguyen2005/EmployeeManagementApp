using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectEmployee.Models
{
    public partial class Request
    {
        public int RequestId { get; set; }

        public int EmployeeId { get; set; }

        public int ManagerId { get; set; }

        public string RequestType { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("raise_amount")] 
        public decimal? RaiseAmount { get; set; }

        public virtual Employee Employee { get; set; } = null!;

        public virtual Employee Manager { get; set; } = null!;
    }
}
