using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectEmployee.Models
{
    public class AttendanceLog
    {
        [Key]
        public int LogId { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public DateTime CheckInTime { get; set; }
        public double SimilarityScore { get; set; }
        public string Status { get; set; } 
    }
}
