using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProjectEmployee.Services;

namespace ProjectEmployee.HR
{
    public partial class AllEmployeesManagementWindow : Window
    {
        private readonly ApContext _context;
        private readonly User _currentUser;

        public AllEmployeesManagementWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _context = new ApContext();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateFilters();
            LoadEmployees();
        }

        private void PopulateFilters()
        {
            var departments = _context.Departments.OrderBy(d => d.DepartmentName).ToList();
            departments.Insert(0, new Department { DepartmentId = 0, DepartmentName = "All Departments" });
            cboDepartmentFilter.ItemsSource = departments;
            cboDepartmentFilter.DisplayMemberPath = "DepartmentName";
            cboDepartmentFilter.SelectedValuePath = "DepartmentId";
            cboDepartmentFilter.SelectedIndex = 0;
            var jobs = _context.Jobs.OrderBy(j => j.JobTitle).ToList();
            jobs.Insert(0, new Job { JobId = 0, JobTitle = "All Job Titles" });
            cboJobFilter.ItemsSource = jobs;
            cboJobFilter.DisplayMemberPath = "JobTitle";
            cboJobFilter.SelectedValuePath = "JobId";
            cboJobFilter.SelectedIndex = 0;
            cboRoleFilter.ItemsSource = new List<string> { "All Roles", "Managers", "Employees" };
            cboRoleFilter.SelectedIndex = 0;
            cboSortBy.ItemsSource = new List<string> { "Default (Name A-Z)", "Salary (High to Low)", "Salary (Low to High)" };
            cboSortBy.SelectedIndex = 0;
        }

        private void LoadEmployees()
        {
            if (!IsLoaded) return;

            try
            {
                var query = _context.Employees.AsQueryable();
                query = query.Where(e => e.Department != null && e.Department.DepartmentName != "Human Resources");
                if (chkShowInactive.IsChecked == false)
                {
                    query = query.Where(e => e.IsActive == true);
                }
                if (cboDepartmentFilter.SelectedValue is int deptId && deptId > 0)
                {
                    query = query.Where(e => e.DepartmentId == deptId);
                }
                if (cboJobFilter.SelectedValue is int jobId && jobId > 0)
                {
                    query = query.Where(e => e.JobId == jobId);
                }

                if (cboRoleFilter.SelectedItem is string selectedRole)
                {
                    if (selectedRole == "Managers")
                    {
                        query = query.Where(e => e.InverseManager.Any());
                    }
                    else if (selectedRole == "Employees")
                    {
                        query = query.Where(e => !e.InverseManager.Any());
                    }
                }

                string searchTerm = txtSearch.Text.ToLower();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(e =>
                        (e.FirstName + " " + e.LastName).ToLower().Contains(searchTerm) ||
                        e.Email.ToLower().Contains(searchTerm)
                    );
                }
                string sortBy = (cboSortBy.SelectedItem as string) ?? "Default (Name A-Z)";
                switch (sortBy)
                {
                    case "Salary (High to Low)":
                        query = query.OrderByDescending(e => e.Salary);
                        break;
                    case "Salary (Low to High)":
                        query = query.OrderBy(e => e.Salary);
                        break;
                    default:
                        query = query.OrderBy(e => e.FirstName);
                        break;
                }

                var employees = query
                    .Include(e => e.Department)
                    .Include(e => e.Job)
                    .Include(e => e.Manager)
                    .Select(e => new
                    {
                        e.EmployeeId,
                        FullName = e.FirstName + " " + e.LastName,
                        e.Email,
                        DepartmentName = e.Department != null ? e.Department.DepartmentName : "N/A",
                        JobTitle = e.Job != null ? e.Job.JobTitle : "N/A",
                        e.Salary,
                        ManagerName = e.Manager != null ? (e.Manager.FirstName + " " + e.Manager.LastName) : "N/A",
                        Status = e.IsActive ? "Active" : "Inactive"
                    }).ToList();

                dgEmployees.ItemsSource = employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading employee data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadEmployees();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cboDepartmentFilter.SelectedIndex = 0;
            cboJobFilter.SelectedIndex = 0;
            cboRoleFilter.SelectedIndex = 0;
            cboSortBy.SelectedIndex = 0;
            chkShowInactive.IsChecked = false;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var addEditWindow = new AddEditEmployeeWindow(null, _currentUser);
            addEditWindow.ShowDialog();
            LoadEmployees();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is object item)
            {
                var empId = (int)item.GetType().GetProperty("EmployeeId").GetValue(item);
                var addEditWindow = new AddEditEmployeeWindow(empId, _currentUser);
                addEditWindow.ShowDialog();
                LoadEmployees();
            }
        }


        private void Deactivate_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is object item)
            {
                var empId = (int)item.GetType().GetProperty("EmployeeId").GetValue(item);
                var employee = _context.Employees
                                    .Include(emp => emp.InverseManager)
                                    .FirstOrDefault(emp => emp.EmployeeId == empId);

                if (employee == null) return;

                bool isManager = employee.InverseManager.Any();

                if (isManager)
                {
                    var reassignWindow = new ReassignManagerWindow(employee, _currentUser);
                    reassignWindow.Owner = this;
                    reassignWindow.ShowDialog();
                }
                else
                {
                    var result = MessageBox.Show($"Are you sure you want to deactivate {employee.FullName}?",
                                                 "Confirm Deactivation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        employee.IsActive = false;
                        _context.SaveChanges();
                    }
                    AuditLogger.Log("Deactivate Employee", _currentUser, $"Deactivated employee: {employee.FullName}");
                }
                LoadEmployees();
            }
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
