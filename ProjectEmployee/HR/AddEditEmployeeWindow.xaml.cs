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

namespace ProjectEmployee.HR
{
    /// <summary>
    /// Interaction logic for AddEditEmployeeWindow.xaml
    /// </summary>
    public partial class AddEditEmployeeWindow : Window
    {
        private readonly ApContext _context;
        private readonly Employee _employeeToEdit;

        public AddEditEmployeeWindow(int? employeeId)
        {
            InitializeComponent();
            _context = new ApContext();
            PopulateComboBoxes();

            if (employeeId.HasValue)
            {
                txtTitle.Text = "Edit Employee Profile";
                _employeeToEdit = _context.Employees.Find(employeeId.Value);
                if (_employeeToEdit != null)
                {
                    LoadEmployeeData();
                }
            }
            else
            {
                txtTitle.Text = "Add New Employee";
                _employeeToEdit = new Employee { IsActive = true, HireDate = DateOnly.FromDateTime(DateTime.Today) };
                dpHireDate.SelectedDate = _employeeToEdit.HireDate.ToDateTime(TimeOnly.MinValue);
            }
        }

        private void LoadEmployeeData()
        {
            txtFirstName.Text = _employeeToEdit.FirstName;
            txtLastName.Text = _employeeToEdit.LastName;
            txtEmail.Text = _employeeToEdit.Email;
            txtPhoneNumber.Text = _employeeToEdit.PhoneNumber;
            dpHireDate.SelectedDate = _employeeToEdit.HireDate.ToDateTime(TimeOnly.MinValue);
            txtSalary.Text = _employeeToEdit.Salary.ToString();
            cboDepartment.SelectedValue = _employeeToEdit.DepartmentId;
            cboJob.SelectedValue = _employeeToEdit.JobId;
            cboManager.SelectedValue = _employeeToEdit.ManagerId;
        }

        private void PopulateComboBoxes()
        {
            cboDepartment.ItemsSource = _context.Departments.OrderBy(d => d.DepartmentName).ToList();
            cboDepartment.DisplayMemberPath = "DepartmentName";
            cboDepartment.SelectedValuePath = "DepartmentId";

            cboJob.ItemsSource = _context.Jobs.OrderBy(j => j.JobTitle).ToList();
            cboJob.DisplayMemberPath = "JobTitle";
            cboJob.SelectedValuePath = "JobId";

            var managers = _context.Employees
                .Where(e => e.InverseManager.Any() && e.IsActive)
                .OrderBy(e => e.FirstName)
                .ToList();
            cboManager.ItemsSource = managers;
            cboManager.DisplayMemberPath = "FullName"; 
            cboManager.SelectedValuePath = "EmployeeId";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("First Name and Last Name are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(txtSalary.Text, out decimal salary) || salary < 0)
            {
                MessageBox.Show("Please enter a valid salary.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _employeeToEdit.FirstName = txtFirstName.Text;
            _employeeToEdit.LastName = txtLastName.Text;
            _employeeToEdit.Email = txtEmail.Text;
            _employeeToEdit.PhoneNumber = txtPhoneNumber.Text;
            _employeeToEdit.HireDate = DateOnly.FromDateTime(dpHireDate.SelectedDate ?? DateTime.Today);
            _employeeToEdit.Salary = decimal.TryParse(txtSalary.Text, out var sal) ? sal : 0;
            _employeeToEdit.DepartmentId = (int?)cboDepartment.SelectedValue;
            _employeeToEdit.JobId = (int)cboJob.SelectedValue;
            _employeeToEdit.ManagerId = (int?)cboManager.SelectedValue;

            try
            {
                if (_employeeToEdit.EmployeeId == 0)
                {
                    _context.Employees.Add(_employeeToEdit);
                }
                _context.SaveChanges();
                MessageBox.Show("Employee data saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
