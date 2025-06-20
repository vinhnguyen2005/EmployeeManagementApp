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

        public ManagerDashboard(User user, ApContext context)
        {
            InitializeComponent();
            _currentUser = user;
            _context = context;

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
            var teamWindow = new TeamManagement(_currentUser, _context);
            teamWindow.Show();
        }


        private void DepartmentStats_Click(object sender, RoutedEventArgs e)
        {
            var statsWindow = new StatisticWindow(_currentUser, _context);
            statsWindow.Show();
        }

        private void ViewDependents_Click(object sender, RoutedEventArgs e)
        {
            var myEmployees = _context.Employees
                .Where(e => e.ManagerId == _currentUser.EmployeeId)
                .Select(e => e.EmployeeId)
                .ToList();

            var dependents = _context.Dependents
                .Where(d => myEmployees.Contains(d.EmployeeId))
                .Select(d => $"{d.FirstName} {d.LastName} - {d.Relationship}")
                .ToList();

            string result = string.Join("\n", dependents);
            MessageBox.Show(result.Length > 0 ? result : "No dependents found.", "Dependents");
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coming soon: Reports dashboard", "Reports");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
