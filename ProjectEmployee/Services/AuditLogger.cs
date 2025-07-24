using ProjectEmployee.Models;
using System;

namespace ProjectEmployee.Services
{
    public static class AuditLogger
    {
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
                Console.WriteLine("Failed to log audit entry. Please check the database connection and configuration.");
            }
        }
    }
}