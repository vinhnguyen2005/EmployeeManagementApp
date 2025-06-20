using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using ProjectEmployee.Formatter;
using ProjectEmployee.Models;

namespace ProjectEmployee
{
    /// <summary>
    /// Interaction logic for TeamManagement.xaml
    /// </summary>
    public partial class TeamManagement : Window
    {
        private readonly User _currentUser;
        private readonly ApContext _context;

        public TeamManagement(User user, ApContext context)
        {
            InitializeComponent();
            _currentUser = user;
            _context = context;
            if (_context == null)
            {
                MessageBox.Show("⚠️ _context is NULL in TeamManagement");
            }
            LoadEmployeeData();
        }

        private void LoadEmployeeData()
        {
            var team = _context.Employees
                .Where(e => e.ManagerId == _currentUser.EmployeeId)
                .Select(e => new
                {
                    e.EmployeeId,
                    FullName = $"{e.FirstName} {e.LastName}",
                    e.Email,
                    e.PhoneNumber,
                    e.HireDate,
                    JobTitle = e.Job.JobTitle,
                    Salary = FormattingHelper.FormatAsInteger(e.Salary), 
                    Department = e.Department.DepartmentName
                })
                .ToList();

            dgEmployees.ItemsSource = team;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployeeData();
        }

        private void NewRequest_Click(object sender, RoutedEventArgs e)
        {
            if (dgEmployees.SelectedItem == null)
            {
                MessageBox.Show("Please select an employee first.");
                return;
            }

            var selected = dgEmployees.SelectedItem;
            var empIdProp = selected.GetType().GetProperty("EmployeeId");
            if (empIdProp == null)
            {
                MessageBox.Show("Unable to retrieve employee ID.");
                return;
            }

            int employeeId = (int)empIdProp.GetValue(selected)!;

            var employee = _context.Employees
                .Include(e => e.Job)
                .Include(e => e.Department)
                .FirstOrDefault(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                MessageBox.Show("Employee not found.");
                return;
            }

            var requestWindow = new RequestWindow(_currentUser, employee, _context);
            requestWindow.ShowDialog();
        }

        private void ViewRequests_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.EmployeeId == null || _context == null)
            {
                MessageBox.Show("Current user does not have an associated Employee ID.");
                return;
            }

            var requestCount = _context.Requests
                .Count(r => r.ManagerId == _currentUser.EmployeeId);

            if (requestCount == 0)
            {
                MessageBox.Show("No requests found for this manager. Please create a request first.");
                return;
            }

            var window = new ViewRequestWindow(_context, _currentUser);
            window.ShowDialog();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            var filteredTeam = _context.Employees
                .Where(e => e.ManagerId == _currentUser.EmployeeId &&
                ((e.FirstName + " " + e.LastName).ToLower().Contains(keyword) ||
                 e.Email.ToLower().Contains(keyword) ||
                 e.PhoneNumber.ToLower().Contains(keyword) ||
                 e.Job.JobTitle.ToLower().Contains(keyword) ||
                 e.Department.DepartmentName.ToLower().Contains(keyword)))
                .Select(e => new
                {
                    FullName = $"{e.FirstName} {e.LastName}",
                    e.Email,
                    e.PhoneNumber,
                    e.HireDate,
                    JobTitle = e.Job.JobTitle,
                    Salary = FormattingHelper.FormatAsInteger(e.Salary), // Định dạng Salary
                    Department = e.Department.DepartmentName
                }).ToList();
            dgEmployees.ItemsSource = filteredTeam;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSearch_Click(sender, e);
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            var dashboard = new ManagerDashboard(_currentUser, _context);
            dashboard.Show();
            this.Close();
        }

        private void dgEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = dgEmployees.SelectedItem;

            if (selected != null)
            {
                var fullNameProp = selected.GetType().GetProperty("FullName");
                var emailProp = selected.GetType().GetProperty("Email");

                string name = fullNameProp?.GetValue(selected)?.ToString() ?? "";
                string email = emailProp?.GetValue(selected)?.ToString() ?? "";

                txtSelectedEmployee.Text = $"Selected: {name} ({email})";
            }
            else
            {
                txtSelectedEmployee.Text = "";
            }
        }
    }
}