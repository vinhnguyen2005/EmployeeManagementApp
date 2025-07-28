using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ProjectEmployee.Services;

namespace ProjectEmployee.Admin
{
    public class HealthCheckResult
    {
        public string Description { get; set; }
        public string Details { get; set; }
    }

    public partial class SystemSettingsWindow : Window
    {
        private readonly ApContext _context;

        public SystemSettingsWindow()
        {
            InitializeComponent();
            _context = new ApContext();
        }

        private void CheckMismatchedManagers_Click(object sender, RoutedEventArgs e)
        {
            txtResultTitle.Text = "Results: Managers without MANAGER Role";
            var results = new List<HealthCheckResult>();

            var mismatchedManagers = _context.Employees
                .Where(e => e.InverseManager.Any() && !e.Users.Any(u => u.UserRoles.Any(ur => ur.Role.RoleName == "MANAGER")))
                .ToList();

            if (!mismatchedManagers.Any())
            {
                results.Add(new HealthCheckResult { Description = "No issues found.", Details = "All employees with subordinates have the MANAGER role." });
            }
            else
            {
                foreach (var emp in mismatchedManagers)
                {
                    results.Add(new HealthCheckResult
                    {
                        Description = $"Mismatch Found: {emp.FullName}",
                        Details = $"Employee ID: {emp.EmployeeId} is a manager but does not have the MANAGER role."
                    });
                }
            }
            lvResults.ItemsSource = results;
        }

        private void CheckInactiveEmployeeUsers_Click(object sender, RoutedEventArgs e)
        {
            txtResultTitle.Text = "Results: Active Users for Inactive Employees";
            var results = new List<HealthCheckResult>();

            var usersToLock = _context.Users
                .Where(u => u.EmployeeId != null && !u.Employee.IsActive && !u.IsLocked)
                .Include(u => u.Employee)
                .ToList();

            if (!usersToLock.Any())
            {
                results.Add(new HealthCheckResult { Description = "No issues found.", Details = "All users of inactive employees are correctly locked." });
            }
            else
            {
                foreach (var user in usersToLock)
                {
                    results.Add(new HealthCheckResult
                    {
                        Description = $"Action Needed: {user.Employee.FullName}",
                        Details = $"User '{user.Username}' account should be locked because the employee is inactive."
                    });
                }
            }
            lvResults.ItemsSource = results;
        }
        private void CheckOrphanedRequests_Click(object sender, RoutedEventArgs e)
        {
            txtResultTitle.Text = "Results: Requests Assigned to Inactive Managers";
            var results = new List<HealthCheckResult>();

            var orphanedRequests = _context.Requests
                .Where(r => r.Status == "Pending" && (r.Manager == null || !r.Manager.IsActive))
                .Include(r => r.Manager)
                .ToList();

            if (!orphanedRequests.Any())
            {
                results.Add(new HealthCheckResult { Description = "No issues found.", Details = "All pending requests are assigned to active managers." });
            }
            else
            {
                foreach (var req in orphanedRequests)
                {
                    results.Add(new HealthCheckResult
                    {
                        Description = $"Orphaned Request ID: {req.RequestId}",
                        Details = $"Type: '{req.RequestType}', assigned to inactive manager: {req.Manager?.FullName ?? "N/A"}"
                    });
                }
            }
            lvResults.ItemsSource = results;
        }
    }
}
