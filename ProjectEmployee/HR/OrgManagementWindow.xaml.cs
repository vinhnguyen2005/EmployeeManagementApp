using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace ProjectEmployee.HR
{
    public partial class OrgManagementWindow : Window
    {
        private readonly ApContext _context;
        private readonly User _currentUser;

        public OrgManagementWindow(User currentUser)
        {
            InitializeComponent();
            _context = new ApContext();
            _currentUser = currentUser; 
            LoadData();
        }

        private void LoadData()
        {
            LoadDepartments();
            LoadJobs();
        }

        private void LoadDepartments()
        {
            using (var refreshContext = new ApContext())
            {
                dgDepartments.ItemsSource = refreshContext.Departments
                    .Include(d => d.Location)
                        .ThenInclude(l => l.Country)
                    .OrderBy(d => d.DepartmentName)
                    .ToList();
            }
        }

        private void LoadJobs()
        {
            using (var refreshContext = new ApContext())
            {
                dgJobs.ItemsSource = refreshContext.Jobs
                    .OrderBy(j => j.JobTitle)
                    .ToList();
            }
        }

        private void AddDepartment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditDepartmentWindow(null);
            if (dialog.ShowDialog() == true)
            {
                LoadDepartments(); 
            }
        }

        private void EditDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (dgDepartments.SelectedItem is Department selectedDept)
            {
                var dialog = new AddEditDepartmentWindow(selectedDept);
                if (dialog.ShowDialog() == true)
                {
                    LoadDepartments();
                }
            }
            else
            {
                MessageBox.Show("Please select a department to edit.");
            }
        }

        private void DeleteDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (dgDepartments.SelectedItem is Department selectedDept)
            {
                bool isDepartmentInUse = _context.Employees.Any(emp => emp.DepartmentId == selectedDept.DepartmentId);
                if (isDepartmentInUse)
                {
                    MessageBox.Show($"Cannot delete '{selectedDept.DepartmentName}'.\nThis department still has employees assigned to it. Please re-assign them first.", "Action Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Are you sure you want to delete the '{selectedDept.DepartmentName}' department?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Departments.Remove(selectedDept);
                    _context.SaveChanges();
                    LoadDepartments();
                }
            }
            else
            {
                MessageBox.Show("Please select a department to delete.");
            }
        }
        private void AddJob_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditJobWindow(null);
            if (dialog.ShowDialog() == true)
            {
                LoadJobs(); 
            }
        }

        private void EditJob_Click(object sender, RoutedEventArgs e)
        {
            if (dgJobs.SelectedItem is Job selectedJob)
            {
                var dialog = new AddEditJobWindow(selectedJob);
                if (dialog.ShowDialog() == true)
                {
                    LoadJobs();
                }
            }
            else
            {
                MessageBox.Show("Please select a job to edit.");
            }
        }

        private void DeleteJob_Click(object sender, RoutedEventArgs e)
        {
            if (dgJobs.SelectedItem is Job selectedJob)
            {
                bool isJobInUse = _context.Employees.Any(emp => emp.JobId == selectedJob.JobId);
                if (isJobInUse)
                {
                    MessageBox.Show($"Cannot delete '{selectedJob.JobTitle}'.\nThis job title is still assigned to employees. Please update their roles first.", "Action Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Are you sure you want to delete the '{selectedJob.JobTitle}' job?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Jobs.Remove(selectedJob);
                    _context.SaveChanges();
                    LoadJobs();
                }
            }
            else
            {
                MessageBox.Show("Please select a job to delete.");
            }
        }
    }
}
