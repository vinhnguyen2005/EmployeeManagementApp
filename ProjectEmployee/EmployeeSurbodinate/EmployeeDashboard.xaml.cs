using ProjectEmployee.Models;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System;
using ProjectEmployee.HR;

namespace ProjectEmployee.EmployeeSurbodinate
{
    public partial class EmployeeDashboard : Window
    {
        private readonly ApContext _context;
        private readonly User _currentUser;
        private readonly Employee _currentEmployee;
        private readonly List<string> _userRoles;
        public EmployeeDashboard(User loggedInUser)
        {
            InitializeComponent();
            _currentUser = loggedInUser;
            _context = new ApContext();
            _currentEmployee = loggedInUser.Employee;

            if (_currentEmployee == null)
            {
                MessageBox.Show("Error: The logged-in user is not associated with an employee record.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }
            _userRoles = _currentUser.UserRoles.Select(ur => ur.Role.RoleName.ToUpper()).ToList();
            CheckForReturnButton();
            LoadDashboardData();
        }
        private void CheckForReturnButton()
        {
            if (_userRoles.Contains("HR"))
            {
                btnReturnToPreviousView.Content = "↩️ Return to HR View";
                btnReturnToPreviousView.Visibility = Visibility.Visible;
            }
            else if (_userRoles.Contains("MANAGER"))
            {
                btnReturnToPreviousView.Content = "↩️ Return to Manager View";
                btnReturnToPreviousView.Visibility = Visibility.Visible;
            }
        }

        private void ReturnToPreviousView_Click(object sender, RoutedEventArgs e)
        {
            if (_userRoles.Contains("HR"))
            {
                new HRDashboard(_currentUser).Show();
            }
            else if (_userRoles.Contains("MANAGER"))
            {
                new ManagerDashboard(_currentUser).Show();
            }

            this.Close();
        }

        private void LoadDashboardData()
        {
            _context.Entry(_currentEmployee).Reference(e => e.Job).Load();
            txtWelcome.Text = $"Welcome back, {_currentEmployee.FirstName}!";
            txtJobTitle.Text = _currentEmployee.Job?.JobTitle ?? "N/A";
            txtPendingTasks.Text = _context.Tasks.Count(t => t.EmployeeId == _currentEmployee.EmployeeId && t.Status != "Completed").ToString();
            txtTotalRequests.Text = _context.Requests.Count(r => r.OriginatorId == _currentEmployee.EmployeeId).ToString();
            txtDependentCount.Text = _context.Dependents.Count(d => d.EmployeeId == _currentEmployee.EmployeeId).ToString();
            var activeTasks = _context.Tasks
                                      .Where(t => t.EmployeeId == _currentEmployee.EmployeeId && t.Status != "Completed")
                                      .OrderBy(t => t.Deadline)
                                      .Take(3)
                                      .ToList();

            lvActiveTasks.ItemsSource = activeTasks;
            tbNoTasks.Visibility = activeTasks.Any() ? Visibility.Collapsed : Visibility.Visible;

            var recentRequests = _context.Requests
                                         .Where(r => r.OriginatorId == _currentEmployee.EmployeeId && r.EmployeeId == _currentEmployee.EmployeeId)
                                         .OrderByDescending(r => r.CreatedAt)
                                         .Take(3)
                                         .ToList();

            lvRecentRequests.ItemsSource = recentRequests;
            tbNoRequests.Visibility = recentRequests.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboardData();
        }

        private void MyTasks_Click(object sender, RoutedEventArgs e)
        {
            var myTasksWindow = new MyTasksWindow(_currentUser);
            myTasksWindow.ShowDialog();
            LoadDashboardData();
        }

        private void MyRequests_Click(object sender, RoutedEventArgs e)
        {
            var myRequestsWindow = new MyRequestsHistoryWindow(_currentUser);
            myRequestsWindow.ShowDialog();
            LoadDashboardData();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void MyProfile_Click(object sender, RoutedEventArgs e)
        {
            var myProfileWindow = new MyProfileWindow(_currentUser);
            myProfileWindow.ShowDialog();
            LoadDashboardData();
        }

        private void CheckAttendance_Click(object sender, RoutedEventArgs e)
        {
            var attendanceWindow = new AttendanceWindow(_currentUser);
            attendanceWindow.ShowDialog();
            LoadDashboardData();
        }
    }
}