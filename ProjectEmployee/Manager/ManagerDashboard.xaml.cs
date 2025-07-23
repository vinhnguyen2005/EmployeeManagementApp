using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProjectEmployee.EmployeeSurbodinate;
using ProjectEmployee.HR; 

namespace ProjectEmployee
{
    public partial class ManagerDashboard : Window
    {
        private readonly User _currentUser;
        private readonly ApContext _context;

        public ManagerDashboard(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _context = new ApContext();
            LoadManagerData();
            CheckAdditionalRoles();
        }

        private void LoadManagerData()
        {
            if (_currentUser.EmployeeId == null)
            {
                MessageBox.Show("Current user is not properly linked to an employee record.", "Error");
                return;
            }
            int managerId = (int)_currentUser.EmployeeId;
            txtWelcome.Text = $"Welcome, {_currentUser.Employee?.FirstName ?? _currentUser.Username}!";
            txtTeamCount.Text = _context.Employees.Count(e => e.ManagerId == managerId && e.IsActive).ToString();
            txtPendingRequests.Text = _context.Requests
                .Count(r => r.OriginatorId == managerId && r.Status == "Pending").ToString();
            txtHighPriorityTasks.Text = _context.Tasks
                .Count(t => t.Employee.ManagerId == managerId && t.Status != "Completed" && (t.Priority == "High" || t.Deadline < DateTime.Today))
                .ToString();
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            txtCompletedTasks.Text = _context.Tasks
                .Count(t => t.Employee.ManagerId == managerId && t.Status == "Completed" && t.CompletedDate >= thirtyDaysAgo)
                .ToString();
            var teamSummary = _context.Employees
                .Where(e => e.ManagerId == managerId && e.IsActive)
                .Include(e => e.Job)
                .Select(e => new { FullName = e.FirstName + " " + e.LastName, JobTitle = e.Job.JobTitle })
                .Take(5) 
                .ToList();

            lvMyTeam.ItemsSource = teamSummary;
            tbNoTeamMembers.Visibility = teamSummary.Any() ? Visibility.Collapsed : Visibility.Visible;

            var sentRequestsSummary = _context.Requests
                .Where(r => r.OriginatorId == managerId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new { r.RequestId, r.RequestType, r.Status, r.CreatedAt })
                .Take(5)
                .ToList();

            lvMySentRequests.ItemsSource = sentRequestsSummary;
            tbNoSentRequests.Visibility = sentRequestsSummary.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CheckAdditionalRoles()
        {
            var userRoles = _currentUser.UserRoles.Select(ur => ur.Role.RoleName.ToUpper()).ToList();
            if (userRoles.Contains("HR"))
            {
                btnReturnToHRView.Visibility = Visibility.Visible;
            }
        }
        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            if (SidebarColumn.Width.Value > 60)
            {
                SidebarColumn.Width = new GridLength(60);
                txtDashboardTitle.Visibility = Visibility.Collapsed;
            }
            else
            {
                SidebarColumn.Width = new GridLength(250);
                txtDashboardTitle.Visibility = Visibility.Visible;
            }
        }

        private void ViewTeam_Click(object sender, RoutedEventArgs e)
        {
            var teamWindow = new TeamManagement(_currentUser);
            teamWindow.ShowDialog();
        }

        private void ViewMyRequests_Click(object sender, RoutedEventArgs e)
        {
            var requestHistoryWindow = new ViewRequestWindow(_context, _currentUser);
            requestHistoryWindow.ShowDialog();
        }

        private void DepartmentStats_Click(object sender, RoutedEventArgs e)
        {
            var statsWindow = new StatisticWindow(_currentUser, _context);
            statsWindow.ShowDialog();
        }

        private void AssignTask_Click(object sender, RoutedEventArgs e)
        {
            var employees = _context.Employees.Where(e => e.ManagerId == _currentUser.EmployeeId && e.IsActive).ToList();
            if (!employees.Any())
            {
                MessageBox.Show("You have no active employees to assign tasks to.");
                return;
            }
            var assignTaskWindow = new AssignTaskWindow(_currentUser, _context, employees);
            assignTaskWindow.ShowDialog();
        }

        private void PerformanceReviews_Click(object sender, RoutedEventArgs e)
        {
            var reviewWindow = new PerformanceReviewWindow(_currentUser);
            reviewWindow.ShowDialog();
        }

        private void ReturnToHRView_Click(object sender, RoutedEventArgs e)
        {
            var hrDashboard = new HRDashboard(_currentUser);
            hrDashboard.Show();
            this.Close();
        }

        private void SwitchToEmployeeView_Click(object sender, RoutedEventArgs e)
        {
            var employeeDashboard = new EmployeeDashboard(_currentUser);
            employeeDashboard.Show();
            this.Close();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings page is not implemented yet.");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
