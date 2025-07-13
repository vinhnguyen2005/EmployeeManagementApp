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

namespace ProjectEmployee.HR
{
    /// <summary>
    /// Interaction logic for HRDashboard.xaml
    /// </summary>
    public partial class HRDashboard : Window
    {
        public class RequestSummaryViewModel
        {
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
            LoadDashboardData();
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
                int currentHrId = (int)_currentUser.EmployeeId;
                txtPendingRequests.Text = _context.Requests
                    .Count(r => r.ManagerId == currentHrId && r.Status == "Pending").ToString();

                var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
                var newHires = _context.Employees.Where(e => e.HireDate >= thirtyDaysAgo).Count();
                txtNewHires.Text = newHires.ToString();
                var pendingRequestsSummary = _context.Requests
                    .Where(r => r.ManagerId == currentHrId && r.Status == "Pending")
                    .Include(r => r.Originator) 
                    .OrderBy(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new RequestSummaryViewModel
                    {
                        RequestType = r.RequestType,
                        OriginatorName = r.Originator.FirstName + " " + r.Originator.LastName,
                        CreatedAt = r.CreatedAt
                    })
                    .ToList();

                lvPendingRequests.ItemsSource = pendingRequestsSummary;
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
            MessageBox.Show("Sẽ mở trang quản lý phòng ban, chức danh.");
            // Ví dụ: new OrgManagementWindow(_currentUser).ShowDialog();
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sẽ mở form thêm nhân viên mới.");
            // Ví dụ: new AddEmployeeWindow().ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
