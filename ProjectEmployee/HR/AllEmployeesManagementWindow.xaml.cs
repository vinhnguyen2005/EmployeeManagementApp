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
using Microsoft.EntityFrameworkCore;
using ProjectEmployee.Models;

namespace ProjectEmployee.HR
{
    /// <summary>
    /// Interaction logic for AllEmployeesManagementWindow.xaml
    /// </summary>
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
            LoadEmployeeData();
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
        }

        private void LoadEmployeeData()
        {
            try
            {
                var query = _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Job)
                    .Include(e => e.Manager)
                    .Where(e => e.Department.DepartmentId != 4);
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
                string searchTerm = txtSearch.Text.ToLower();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(e =>
                            (e.FirstName + " " + e.LastName).ToLower().Contains(searchTerm) ||
                            e.Email.ToLower().Contains(searchTerm) ||
                            (e.PhoneNumber != null && e.PhoneNumber.Contains(searchTerm)) ||
                            e.Job.JobTitle.ToLower().Contains(searchTerm) ||
                            e.Department.DepartmentName.ToLower().Contains(searchTerm)
                        );
                }

                var employees = query
                    .OrderBy(e => e.FirstName)
                    .Select(e => new
                    {
                        e.EmployeeId,
                        FullName = e.FirstName + " " + e.LastName,
                        e.Email,
                        DepartmentName = e.Department.DepartmentName,
                        JobTitle = e.Job.JobTitle,
                        ManagerName = e.Manager != null ? (e.Manager.FirstName + " " + e.Manager.LastName) : "N/A",
                        Status = e.IsActive ? "Active" : "Inactive"
                    }).ToList();

                dgEmployees.ItemsSource = employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            LoadEmployeeData();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadEmployeeData();
        }


        private void Deactivate_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is object item)
            {
                var empId = (int)item.GetType().GetProperty("EmployeeId")?.GetValue(item);
                var employee = _context.Employees.Find(empId);
                if (employee == null)
                {
                    MessageBox.Show("Employee not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var result = MessageBox.Show($"Are you sure you want to deactivate {employee.FirstName} {employee.LastName}?",
                                                  "Confirm Deactivation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    employee.IsActive = false;
                    _context.SaveChanges();
                    LoadEmployeeData();
                }
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var addEditWindow = new AddEditEmployeeWindow(null);
            addEditWindow.ShowDialog();
            LoadEmployeeData();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is object item)
            {
                var empId = (int)item.GetType().GetProperty("EmployeeId").GetValue(item);
                var addEditWindow = new AddEditEmployeeWindow(empId);
                addEditWindow.ShowDialog();
                LoadEmployeeData();
            }
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cboDepartmentFilter.SelectedIndex = 0;
            cboJobFilter.SelectedIndex = 0;
            chkShowInactive.IsChecked = false;
            LoadEmployeeData();
        }

    }
}
