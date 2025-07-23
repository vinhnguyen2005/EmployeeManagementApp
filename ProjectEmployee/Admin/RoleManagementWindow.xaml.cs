using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using ProjectEmployee.Services;

namespace ProjectEmployee.Admin
{
    public partial class RoleManagementWindow : Window
    {
        private readonly ApContext _context;
        private readonly User _userToEdit;
        private readonly List<Role> _allRoles;

        public RoleManagementWindow(User user)
        {
            InitializeComponent();
            _context = new ApContext();
            _userToEdit = _context.Users
                            .Include(u => u.Employee.Job)
                            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role) 
                            .FirstOrDefault(u => u.UserId == user.UserId);


            if (_userToEdit == null)
            {
                MessageBox.Show("User not found.");
                this.Close();
                return;
            }

            LoadUserInfo();
            _allRoles = _context.Roles.Where(r => r.RoleName != "ADMIN").OrderBy(r => r.RoleName).ToList();
            PopulateRoleCheckBoxes();
        }

        private void LoadUserInfo()
        {
            if (_userToEdit.Employee != null)
            {
                txtFullName.Text = _userToEdit.Employee.FullName;
                txtJobTitle.Text = _userToEdit.Employee.Job?.JobTitle ?? "N/A";
            }
            else
            {
                txtFullName.Text = _userToEdit.Username;
                txtJobTitle.Text = "System Account";
            }
        }

        private void PopulateRoleCheckBoxes()
        {
            var currentUserRoleIds = _userToEdit.UserRoles.Select(ur => ur.RoleId).ToList();
            bool isSystemAdmin = _userToEdit.Username.ToLower() == "admin";

            foreach (var role in _allRoles)
            {
                var checkBox = new CheckBox
                {
                    Content = role.RoleName,
                    Tag = role.RoleId,
                    Margin = new Thickness(0, 5, 0, 5),
                    IsChecked = currentUserRoleIds.Contains(role.RoleId)
                };
                if (role.RoleName == "EMPLOYEE")
                {
                    if (!isSystemAdmin)
                    {
                        checkBox.IsChecked = true;
                        checkBox.IsEnabled = false;
                    }
                }
                spRoles.Children.Add(checkBox);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var selectedRoleIds = new List<int>();
            foreach (CheckBox checkBox in spRoles.Children)
            {
                if (checkBox.IsChecked == true)
                {
                    selectedRoleIds.Add((int)checkBox.Tag);
                }
            }

            if (!selectedRoleIds.Any())
            {
                MessageBox.Show("A user must have at least one role.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var managerRole = _context.Roles.FirstOrDefault(r => r.RoleName == "MANAGER");
            if (managerRole != null)
            {
                bool wasManager = _context.UserRoles.Any(ur => ur.UserId == _userToEdit.UserId && ur.RoleId == managerRole.RoleId);
                bool isStillManager = selectedRoleIds.Contains(managerRole.RoleId);

                if (wasManager && !isStillManager)
                {
                    bool hasSubordinates = _context.Employees.Any(emp => emp.ManagerId == _userToEdit.EmployeeId);
                    if (hasSubordinates)
                    {
                        MessageBox.Show($"Cannot remove 'MANAGER' role from '{_userToEdit.Username}'.\n\nThis user still has employees reporting to them. Please use the HR's 'Deactivate' or 'Re-assign Team' function to handle the subordinates first.", "Action Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            try
            {
                var oldRoles = _context.UserRoles.Where(ur => ur.UserId == _userToEdit.UserId);
                _context.UserRoles.RemoveRange(oldRoles);
                foreach (var roleId in selectedRoleIds)
                {
                    _context.UserRoles.Add(new UserRole { UserId = _userToEdit.UserId, RoleId = roleId });
                }
                _context.SaveChanges();
                MessageBox.Show("User roles updated successfully.", "Success");
                AuditLogger.Log("Update Roles", _userToEdit, $"Updated roles for user: {_userToEdit.Username}");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save roles: {ex.Message}", "Error");
            }
        }
    }
}
