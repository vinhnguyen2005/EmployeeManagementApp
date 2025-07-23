using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using ProjectEmployee.Services;

namespace ProjectEmployee.Admin
{
    public class AdminUserViewModel
    {
        public int UserId { get; set; }
        public int? EmployeeId { get; set; }
        public string Username { get; set; }
        public string EmployeeName { get; set; }
        public string Roles { get; set; }
        public string Status { get; set; }
        public bool HasAccount { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; } 
    }

    public partial class AdminDashboard : Window
    {
        private readonly ApContext _context;
        private readonly User _currentUser;

        public AdminDashboard(User loggedInUser)
        {
            InitializeComponent();
            _context = new ApContext();
            _currentUser = loggedInUser;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                txtTotalUsers.Text = _context.Users.Count().ToString();
                txtLockedAccounts.Text = _context.Users.Count(u => u.IsLocked).ToString();
                txtUsersToCreate.Text = _context.Employees.Count(e => e.IsActive && !e.Users.Any()).ToString();
                var actionRequiredList = new List<AdminUserViewModel>();
                var employeesNeedingAccount = _context.Employees
                    .Where(e => e.IsActive && !e.Users.Any())
                    .ToList();
                actionRequiredList.AddRange(employeesNeedingAccount.Select(emp => new AdminUserViewModel
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeName = emp.FullName,
                    Status = "Needs Account Creation",
                    Roles = string.Join(", ", emp.Users.FirstOrDefault()?.UserRoles.Select(ur => ur.Role.RoleName) ?? new List<string>()),
                    HasAccount = false,
                    IsActive = true
                }));

                var usersToPurge = _context.Users
                  .Include(u => u.Employee)
                  .Include(u => u.UserRoles).ThenInclude(ur => ur.Role) 
                  .Where(u => u.EmployeeId != null && !u.Employee.IsActive)
                  .ToList();
                actionRequiredList.AddRange(usersToPurge.Select(user => new AdminUserViewModel
                {
                    UserId = user.UserId,
                    EmployeeId = user.EmployeeId,
                    Username = user.Username,
                    EmployeeName = user.Employee.FullName,
                    Roles = string.Join(", ", user.UserRoles.Select(ur => ur.Role.RoleName)),
                    Status = "Deactivated - Ready to Purge",
                    HasAccount = true,
                    IsActive = false
                }));

                if (!actionRequiredList.Any())
                {
                    txtActionTitle.Text = "Recently Added Employees";
                    var recentHires = _context.Employees
                        .Where(e => e.IsActive)
                        .OrderByDescending(e => e.HireDate)
                        .Take(5)
                        .Include(e => e.Users).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                        .ToList();

                    dgUsers.ItemsSource = recentHires.Select(emp => new AdminUserViewModel
                    {
                        UserId = emp.Users.FirstOrDefault()?.UserId ?? 0,
                        EmployeeId = emp.EmployeeId,
                        EmployeeName = emp.FullName,
                        Username = emp.Users.FirstOrDefault()?.Username ?? "(No Account)",
                        Roles = string.Join(", ", emp.Users.FirstOrDefault()?.UserRoles.Select(ur => ur.Role.RoleName) ?? new List<string>()),
                        Status = emp.Users.FirstOrDefault()?.IsLocked ?? false ? "Locked" : "Active",
                        HasAccount = emp.Users.Any(),
                        IsActive = true,
                        IsLocked = emp.Users.FirstOrDefault()?.IsLocked ?? false
                    }).ToList();
                }
                else
                {
                    txtActionTitle.Text = "Action Required";
                    dgUsers.ItemsSource = actionRequiredList.OrderBy(u => u.Status).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private void UserManagement_Click(object sender, RoutedEventArgs e)
        {
            var userManagementWindow = new UserManagementWindow(_currentUser);
            userManagementWindow.ShowDialog();
            LoadData(); 
        }

        private void SystemSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SystemSettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void SystemLogs_Click(object sender, RoutedEventArgs e)
        {
            var logsWindow = new SystemLogsWindow();
            logsWindow.ShowDialog();
        }


        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not AdminUserViewModel selected) return;

            var employee = _context.Employees.Find(selected.EmployeeId);
            if (employee == null) return;

            var result = MessageBox.Show($"Create an account for {employee.FullName}?", "Confirm Account Creation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                string username = employee.Email.Split('@')[0].ToLower();
                string tempPassword = GenerateRandomPassword();

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = tempPassword, 
                    EmployeeId = employee.EmployeeId,
                    IsLocked = false
                };
                _context.Users.Add(newUser);
                _context.SaveChanges(); 

                var employeeRole = _context.Roles.FirstOrDefault(r => r.RoleName == "EMPLOYEE");
                if (employeeRole != null)
                {
                    _context.UserRoles.Add(new UserRole { UserId = newUser.UserId, RoleId = employeeRole.RoleId });
                    _context.SaveChanges();
                }

                MessageBox.Show($"Account created successfully!\n\nUsername: {username}\nTemporary Password: {tempPassword}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                AuditLogger.Log("Create Account", _currentUser, $"Created account for Employee ID: {employee.EmployeeId}");
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create account: {ex.Message}", "Error");
            }
        }

        private void ToggleLock_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not AdminUserViewModel selected) return;

            var user = _context.Users.Find(selected.UserId);
            if (user == null) return;

            user.IsLocked = !user.IsLocked; 
            _context.SaveChanges();

            MessageBox.Show($"User '{user.Username}' has been {(user.IsLocked ? "Locked" : "Unlocked")}.", "Success");
            AuditLogger.Log("Toggle Lock", _currentUser, $"{(user.IsLocked ? "Locked" : "Unlocked")} account for user: {user.Username}");
            LoadData();
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not AdminUserViewModel selected) return;

            var user = _context.Users.Find(selected.UserId);
            if (user == null) return;

            string newPassword = GenerateRandomPassword();
            user.PasswordHash = newPassword;
            _context.SaveChanges();

            MessageBox.Show($"Password for '{user.Username}' has been reset.\n\nNew Temporary Password: {newPassword}", "Password Reset", MessageBoxButton.OK, MessageBoxImage.Information);
            AuditLogger.Log("Reset Password", _currentUser, $"Reset password for user: {user.Username}");
        }

        private void ManageRoles_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not AdminUserViewModel selected) return;
            using var context = new ApContext();
            var user = context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefault(u => u.UserId == selected.UserId);
            if (user == null) return;

            var roleManagementWindow = new RoleManagementWindow(user);
            if (roleManagementWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }
        


        private void PurgeAccount_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not AdminUserViewModel selected) return;

            var result = MessageBox.Show($"Are you sure you want to permanently delete the user account for '{selected.EmployeeName}'?\n\nThis action CANNOT be undone.", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            var user = _context.Users.Find(selected.UserId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                MessageBox.Show("User account has been purged.", "Success");
                LoadData();
            }
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
