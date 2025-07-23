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
using ProjectEmployee.Models;
using ProjectEmployee.Services;

namespace ProjectEmployee.HR
{
    /// <summary>
    /// Interaction logic for ReassignManagerWindow.xaml
    /// </summary>
    public partial class ReassignManagerWindow : Window
    {
        private readonly ApContext _context;
        private readonly Employee _managerToDeactivate;
        private readonly User _currentUser;

        public ReassignManagerWindow(Employee managerToDeactivate, User currentUser)
        {
            InitializeComponent();
            _context = new ApContext();
            _currentUser = currentUser;
            _managerToDeactivate = managerToDeactivate;
            LoadData();
        }

        private void LoadData()
        {
            txtInfo.Text = $"You are deactivating manager '{_managerToDeactivate.FullName}'. Please choose a replacement to take over their team of {_managerToDeactivate.InverseManager.Count} member(s).";
            cboExistingManagers.ItemsSource = _context.Employees
                .Where(e => e.IsActive && e.EmployeeId != _managerToDeactivate.EmployeeId && e.InverseManager.Any())
                .OrderBy(e => e.FirstName)
                .ToList();
            cboExistingManagers.DisplayMemberPath = "FullName";
            cboExistingManagers.SelectedValuePath = "EmployeeId";

            cboTeamMembers.ItemsSource = _context.Employees
                .Where(e => e.ManagerId == _managerToDeactivate.EmployeeId && e.IsActive)
                .OrderBy(e => e.FirstName)
                .ToList();
            cboTeamMembers.DisplayMemberPath = "FullName";
            cboTeamMembers.SelectedValuePath = "EmployeeId";
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            int? newManagerId = null;
            Employee employeeToPromote = null;

            if (rbExistingManager.IsChecked == true)
            {
                if (cboExistingManagers.SelectedValue == null)
                {
                    MessageBox.Show("Please select an existing manager.");
                    return;
                }
                newManagerId = (int)cboExistingManagers.SelectedValue;
            }
            else
            {
                if (cboTeamMembers.SelectedValue == null)
                {
                    MessageBox.Show("Please select a team member to promote.");
                    return;
                }
                newManagerId = (int)cboTeamMembers.SelectedValue;
                employeeToPromote = cboTeamMembers.SelectedItem as Employee;
            }
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var teamToReassign = _context.Employees
                    .Where(e => e.ManagerId == _managerToDeactivate.EmployeeId)
                    .ToList();
                foreach (var member in teamToReassign)
                {
                    if (member.EmployeeId != newManagerId)
                    {
                        member.ManagerId = newManagerId;
                    }
                }
                if (employeeToPromote != null)
                {
                    employeeToPromote.ManagerId = _managerToDeactivate.ManagerId;
                    var userToUpdate = _context.Users.FirstOrDefault(u => u.EmployeeId == employeeToPromote.EmployeeId);
                    if (userToUpdate != null)
                    {
                        var employeeRole = _context.Roles.FirstOrDefault(r => r.RoleId == 4);
                        var managerRole = _context.Roles.FirstOrDefault(r => r.RoleId == 3);
                        if(employeeRole != null && managerRole != null)
                        {
                            var oldRoleLink = _context.UserRoles.FirstOrDefault(ur => ur.UserId == userToUpdate.UserId && ur.RoleId == employeeRole.RoleId);
                            if (oldRoleLink != null)
                            {
                                _context.UserRoles.Remove(oldRoleLink);
                            }
                            if(!_context.UserRoles.Any(ur => ur.UserId == userToUpdate.UserId && ur.RoleId == managerRole.RoleId))
                            {
                                _context.UserRoles.Add(new UserRole
                                {
                                    UserId = userToUpdate.UserId,
                                    RoleId = managerRole.RoleId
                                });
                            }
                        }

                    }
                }
                var pendingRequestsForOldManager = _context.Requests
                  .Where(r => r.ManagerId == _managerToDeactivate.EmployeeId && r.Status == "Pending")
                  .ToList();
                foreach (var req in pendingRequestsForOldManager)
                {
                    req.ManagerId = (int)newManagerId;
                }
                var requestsByOldManager = _context.Requests
                              .Where(r => r.OriginatorId == _managerToDeactivate.EmployeeId && r.Status == "Pending");

                if (requestsByOldManager.Any())
                {
                    _context.Requests.RemoveRange(requestsByOldManager);
                }
                _managerToDeactivate.IsActive = false;
                _context.Employees.Update(_managerToDeactivate);
                _context.SaveChanges();
                transaction.Commit();
                MessageBox.Show("Team re-assignment and manager deactivation completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                AuditLogger.Log("Re-assign Team", _currentUser, $"Deactivated manager '{_managerToDeactivate.FullName}' and re-assigned team to new manager ID: {newManagerId}.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reassigning team: {ex.Message}");
                transaction.Rollback();
                return;

            }
           
        }
    }
}
