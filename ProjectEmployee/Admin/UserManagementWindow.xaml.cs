using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using ProjectEmployee.Models;
using ProjectEmployee.Services;

namespace ProjectEmployee.Admin
{
    /// <summary>
    /// Interaction logic for UserManagementWindow.xaml
    /// </summary>
    public partial class UserManagementWindow : Window
    {
        private readonly ApContext _context;
        private readonly User _currentUser;

        public UserManagementWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _context = new ApContext();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateFilters();
            LoadUsers();
        }

        private void PopulateFilters()
        {
            cboStatusFilter.ItemsSource = new List<string> { "All", "Active", "Deactivated", "Locked", "Needs Account" };
            cboStatusFilter.SelectedIndex = 0;

            using var context = new ApContext();
            var roles = context.Roles
                .Where(r => r.RoleName != "ADMIN" && r.RoleName != "EMPLOYEE")
                .OrderBy(r => r.RoleName)
                .ToList();

            roles.Insert(0, new Role { RoleId = 0, RoleName = "All Roles" });

            cboRoleFilter.ItemsSource = roles;
            cboRoleFilter.DisplayMemberPath = "RoleName";
            cboRoleFilter.SelectedValuePath = "RoleId";
            cboRoleFilter.SelectedIndex = 0;
        }

        private void LoadUsers()
        {
            try
            {
                using var context = new ApContext();
                var query = context.Employees
                    .Include(e => e.Users)
                        .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .AsQueryable();


                string statusFilter = (cboStatusFilter.SelectedItem as string) ?? "All";
                string searchTerm = txtSearch.Text.ToLower();
                int selectedRoleId = (cboRoleFilter.SelectedItem as Role)?.RoleId ?? 0;

                var allUsersViewModel = query.ToList().Select(emp =>
                {
                    var user = emp.Users.FirstOrDefault();
                    return new AdminUserViewModel
                    {
                        UserId = user?.UserId ?? 0,
                        EmployeeId = emp.EmployeeId,
                        Username = user?.Username ?? "(No Account)",
                        EmployeeName = emp.FullName,
                        Roles = user != null ? string.Join(", ", user.UserRoles.Select(ur => ur.Role.RoleName)) : "N/A",
                        Status = user == null ? "Needs Account" : (user.IsLocked ? "Locked" : (emp.IsActive ? "Active" : "Deactivated")),
                        HasAccount = user != null,
                        IsActive = emp.IsActive,
                        IsLocked = user?.IsLocked ?? false
                    };
                }).AsQueryable();

                if (statusFilter != "All")
                {
                    allUsersViewModel = allUsersViewModel.Where(u => u.Status == statusFilter);
                }
                if (selectedRoleId > 0)
                {
                    var selectedRoleName = (cboRoleFilter.SelectedItem as Role).RoleName;
                    allUsersViewModel = allUsersViewModel.Where(u => u.Roles.Contains(selectedRoleName));
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    allUsersViewModel = allUsersViewModel.Where(u => u.EmployeeName.ToLower().Contains(searchTerm) || u.Username.ToLower().Contains(searchTerm));
                }

                dgUsers.ItemsSource = allUsersViewModel.OrderBy(u => u.EmployeeName).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void Filters_Changed(object sender, RoutedEventArgs e)
        {
            if (IsLoaded) 
            {
                LoadUsers();
            }
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
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
                LoadUsers();
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
            LoadUsers();
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
                LoadUsers();
            }
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
                LoadUsers(); 
            }
        }


    }
}
