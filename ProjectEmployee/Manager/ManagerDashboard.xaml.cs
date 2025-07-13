using System;
using System.Linq;
using System.Windows;
using ProjectEmployee.Models;
using static System.Windows.Forms.AxHost;

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
        }

        private void LoadManagerData()
        {
            // Show welcome message
            txtWelcome.Text = $"Welcome, {_currentUser.Username}";
            txtTeamCount.Text = _context.Employees
                .Count(e => e.ManagerId == _currentUser.EmployeeId)
                .ToString();
        }

        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            // Collapse or expand the sidebar
            if (SidebarColumn.Width.Value > 50)
                SidebarColumn.Width = new GridLength(50);
            else
                SidebarColumn.Width = new GridLength(200);
        }
        private void ViewTeam_Click(object sender, RoutedEventArgs e)
        {
            var teamWindow = new TeamManagement(_currentUser);
            teamWindow.Show();
        }


        private void DepartmentStats_Click(object sender, RoutedEventArgs e)
        {
            var statsWindow = new StatisticWindow(_currentUser, _context);
            statsWindow.Show();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AssignTask_Click(object sender, RoutedEventArgs e)
        {
            int? managerId = _currentUser.EmployeeId;

            if (managerId.HasValue)
            {
                using (var tempContext = new ApContext())
                {
                    var employees = tempContext.Employees.Where(e => e.ManagerId == managerId.Value && e.IsActive == true).ToList();
                    if (!employees.Any())
                    {
                        MessageBox.Show("No employees found for task assignment.");
                        return;
                    }
                    var assignTaskWindow = new AssignTaskWindow(_currentUser, _context, employees);
                    assignTaskWindow.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Current user does not have an associated Employee ID.");
            }
        }


        private void PerformanceReviews_Click(object sender, RoutedEventArgs e)
        {
            var reviewWindow = new PerformanceReviewWindow(_currentUser);
            reviewWindow.ShowDialog();

        }

    }
}
