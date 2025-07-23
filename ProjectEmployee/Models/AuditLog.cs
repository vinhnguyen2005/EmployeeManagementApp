using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectEmployee.Models
{
    public partial class AuditLog
    {
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; }
        public string ActionType { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
