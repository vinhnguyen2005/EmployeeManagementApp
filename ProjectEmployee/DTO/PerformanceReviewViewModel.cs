using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectEmployee.NewFolder
{
    public class PerformanceReviewViewModel
    {
        // Employee Information
        public string EmployeeInfo { get; set; } 
        public int EmployeeId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; }
        public string TaskStatus { get; set; }
        public string TaskComment { get; set; }

        public decimal TaskScore { get; set; }
        public string TaskScoreLabel { get; set; }
        public string TaskScoreColor { get; set; }
        public string AvgScoreLabel { get; set; }
        public string AvgScoreColor { get; set; }

        public bool IsFirstTaskForEmployee { get; set; }
    }
}