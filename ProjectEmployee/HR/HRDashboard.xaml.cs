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
using Microsoft.EntityFrameworkCore;
using ProjectEmployee.EmployeeSurbodinate;

namespace ProjectEmployee.HR
{
    /// <summary>
    /// Interaction logic for HRDashboard.xaml
    /// </summary>
    public partial class HRDashboard : Window
    {
        public class RequestSummaryViewModel
        {
            public int RequestId { get; set; }
            public string RequestType { get; set; }
            public string OriginatorName { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private readonly ApContext _context;
        private readonly User _currentUser;

        public HRDashboard(User loggedInUser)
        {
            InitializeComponent();
            _context = new ApContext();
            _currentUser = loggedInUser;
            CheckAdditionalRoles();
            LoadDashboardData();
        }

        private void CheckAdditionalRoles()
        {
            var userRoles = _currentUser.UserRoles.Select(ur => ur.Role.RoleName.ToUpper()).ToList();
            if (userRoles.Contains("MANAGER")   )
            {
                btnSwitchToManagerView.Visibility = Visibility.Visible;
            }
        }

        private void SwitchToManagerView_Click(object sender, RoutedEventArgs e)
        {
            var managerDashboard = new ManagerDashboard(_currentUser);
            managerDashboard.Show();
            this.Close();
        }

        private void SwitchToEmployeeView_Click(object sender, RoutedEventArgs e)
        {
            var employeeDashboard = new EmployeeDashboard(_currentUser);
            employeeDashboard.Show();
            this.Close();
        }

        private void LoadDashboardData()
        {
            if (_currentUser.EmployeeId == null)
            {
                MessageBox.Show("Current HR user is not linked to an employee record.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                txtWelcome.Text = $"Welcome, {_currentUser.Employee?.FirstName ?? _currentUser.Username}!";
                txtTotalEmployees.Text = _context.Employees.Count().ToString();
                txtTotalDepartments.Text = _context.Departments.Count().ToString();
                var hrEmployeeIds = _context.UserRoles
                 .Where(ur => ur.Role.RoleId == 2 && ur.User.EmployeeId != null)
                 .Select(ur => (int)ur.User.EmployeeId)
                 .ToList();

                txtPendingRequests.Text = _context.Requests
                    .Count(r => hrEmployeeIds.Contains(r.ManagerId) && r.Status == "Pending").ToString();

                var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
                var newHires = _context.Employees.Where(e => e.HireDate >= thirtyDaysAgo).Count();
                txtNewHires.Text = newHires.ToString();
                if (hrEmployeeIds.Any())
                {
                    var pendingRequestsSummary = _context.Requests
                        .Where(r => hrEmployeeIds.Contains(r.ManagerId) && r.Status == "Pending")
                        .Include(r => r.Originator)
                        .OrderBy(r => r.CreatedAt)
                        .Take(5)
                        .Select(r => new RequestSummaryViewModel
                        {
                            RequestId = r.RequestId,
                            RequestType = r.RequestType,
                            OriginatorName = r.Originator.FirstName + " " + r.Originator.LastName,
                            CreatedAt = r.CreatedAt
                        })
                        .ToList();
                    lvPendingRequests.ItemsSource = pendingRequestsSummary;
                }
                else
                {
                    lvPendingRequests.ItemsSource = null;
                }

                lvRecentHires.ItemsSource = _context.Employees
                    .OrderByDescending(e => e.HireDate)
                    .Include(e => e.Job)
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboardData();
        }

        private void RequestInbox_Click(object sender, RoutedEventArgs e)
        {
            var requestWindow = new RequestApprovalWindow(_currentUser);
            requestWindow.ShowDialog();
            LoadDashboardData(); 
        }
        private void EmployeeManagement_Click(object sender, RoutedEventArgs e)
        {
            var employeeManagementWindow = new AllEmployeesManagementWindow(_currentUser);
            employeeManagementWindow.ShowDialog();
            LoadDashboardData();
        }

        private void OrgStructure_Click(object sender, RoutedEventArgs e)
        {
            var orgWindow = new OrgManagementWindow(_currentUser);
            orgWindow.ShowDialog();
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var addEditWindow = new AddEditEmployeeWindow(null, _currentUser);
            addEditWindow.Owner = this;
            addEditWindow.ShowDialog();
            LoadDashboardData();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();

        }

        private void ViewProfile_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is object item)
            {
                var empId = (int)item.GetType().GetProperty("EmployeeId").GetValue(item);
                var profileWindow = new EmployeeProfileWindow(empId);
                profileWindow.ShowDialog();
            }
        }
    }
}
