using ProjectEmployee.Models;
using System;

namespace ProjectEmployee.Services
{
    public static class AuditLogger
    {
        /// <param name="actionType">Loại hành động (ví dụ: "User Login", "Update Employee").</param>
        /// <param name="user">Người dùng thực hiện hành động. Có thể là null cho các sự kiện hệ thống.</param>
        /// <param name="details">Mô tả chi tiết về hành động.</param>
        public static void Log(string actionType, User user, string details)
        {
            try
            {
                using var context = new ApContext();

                var log = new AuditLog
                {
                    UserId = user?.UserId,
                    Username = user?.Username ?? "System", 
                    ActionType = actionType,
                    Details = details,
                    Timestamp = DateTime.Now
                };

                context.AuditLogs.Add(log);
                context.SaveChanges();
            }
            catch (Exception)
            {

            }
        }
    }
}
