using ProjectEmployee.Models;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System;

namespace ProjectEmployee.EmployeeSurbodinate
{
    public partial class EmployeeDashboard : Window
    {
        private readonly ApContext _context;
        private readonly User _currentUser;
        private readonly Employee _currentEmployee;

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

            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            _context.Entry(_currentEmployee).Reference(e => e.Job).Load();

            // Update welcome text and job title
            txtWelcome.Text = $"Welcome back, {_currentEmployee.FirstName}!";
            txtJobTitle.Text = _currentEmployee.Job?.JobTitle ?? "N/A";

            // Update counters
            txtPendingTasks.Text = _context.Tasks.Count(t => t.EmployeeId == _currentEmployee.EmployeeId && t.Status != "Completed").ToString();
            txtTotalRequests.Text = _context.Requests.Count(r => r.OriginatorId == _currentEmployee.EmployeeId).ToString();

            // Load active tasks
            var activeTasks = _context.Tasks
                                      .Where(t => t.EmployeeId == _currentEmployee.EmployeeId && t.Status != "Completed")
                                      .OrderBy(t => t.Deadline)
                                      .Take(3)
                                      .ToList();

            lvActiveTasks.ItemsSource = activeTasks;
            tbNoTasks.Visibility = activeTasks.Any() ? Visibility.Collapsed : Visibility.Visible;

            // Load recent requests
            var recentRequests = _context.Requests
                                         .Where(r => r.OriginatorId == _currentEmployee.EmployeeId)
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
    }
}