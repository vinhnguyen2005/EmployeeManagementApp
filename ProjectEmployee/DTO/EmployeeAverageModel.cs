using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectEmployee.DTO
{
    public class EmployeeAverageViewModel
    {
        public string EmployeeName { get; set; }
        public int TaskCount { get; set; }
        public decimal AvgScore { get; set; }
    }

}
