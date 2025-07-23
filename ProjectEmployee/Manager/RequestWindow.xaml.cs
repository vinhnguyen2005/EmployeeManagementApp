using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using ProjectEmployee.Formatter;
using ProjectEmployee.Models;

namespace ProjectEmployee
{
    public partial class RequestWindow : Window
    {
        private readonly User _currentUser;
        private readonly Employee _selectedEmployee;
        private readonly ApContext _context;

        public RequestWindow(User currentUser, Employee selectedEmployee)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _context = new ApContext();
            _selectedEmployee = _context.Employees
                .Include(e => e.Job)
                .FirstOrDefault(e => e.EmployeeId == selectedEmployee.EmployeeId);

            if (_selectedEmployee == null)
            {
                MessageBox.Show("Could not load employee details.");
                this.Close();
                return;
            }

            txtEmployeeName.Text = $"👤 {_selectedEmployee.FirstName} {_selectedEmployee.LastName}";
            txtEmployeeEmail.Text = $"✉️ {_selectedEmployee.Email}";
            txtEmployeeJob.Text = $"💼 {_selectedEmployee.Job.JobTitle}";
            txtEmployeeSalary.Text = $"💰 {FormattingHelper.FormatAsInteger(_selectedEmployee.Salary)}";
        }

        private void Raise_Click(object sender, RoutedEventArgs e)
        {
            lblType.Text = "Raise Salary";
            txtDescription.Text = $"Requesting salary raise for {_selectedEmployee.FirstName} {_selectedEmployee.LastName}.";
            raiseAmountPanel.Visibility = Visibility.Visible;
        }

        private void Kick_Click(object sender, RoutedEventArgs e)
        {
            lblType.Text = "Request Termination";
            txtDescription.Text = $"Suggesting termination of {_selectedEmployee.FirstName} {_selectedEmployee.LastName}.";
            raiseAmountPanel.Visibility = Visibility.Collapsed;
        }

        private void SubmitRequest_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblType.Text))
            {
                MessageBox.Show("Please choose a request type.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Please enter a description.");
                return;
            }

            string type = lblType.Text;
            string description = txtDescription.Text.Trim();
            decimal? raiseAmount = null;

            if (type == "Raise Salary")
            {
                if (string.IsNullOrWhiteSpace(txtRaiseAmount.Text))
                {
                    MessageBox.Show("Please enter the raise amount.");
                    return;
                }

                if (!decimal.TryParse(txtRaiseAmount.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Please enter a valid positive raise amount.");
                    return;
                }
                decimal newSalary = _selectedEmployee.Salary + amount;
                if (_selectedEmployee.Job.MaxSalary.HasValue && newSalary > _selectedEmployee.Job.MaxSalary)
                {
                    MessageBox.Show($"The proposed new salary ({newSalary:C0}) exceeds the maximum salary ({_selectedEmployee.Job.MaxSalary:C0}) for the job title '{_selectedEmployee.Job.JobTitle}'.",
                                    "Salary Cap Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                raiseAmount = amount;
                description += $" Requested raise amount: {raiseAmount}.";
            }

            var hrApprover = _context.Users
                     .Include(u => u.Employee)
                     .FirstOrDefault(u => u.UserRoles.Any(ur => ur.Role.RoleId == 2) && u.EmployeeId != null);

            if (hrApprover == null)
            {
                MessageBox.Show("No active HR user found to handle the request. Please contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int hrApproverId = (int)hrApprover.EmployeeId;
            var request = new Request
            {
                OriginatorId = _currentUser.EmployeeId,
                EmployeeId = _selectedEmployee.EmployeeId,
                ManagerId = hrApproverId,
                RequestType = type,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                RaiseAmount = raiseAmount
            };

            _context.Requests.Add(request);
            _context.SaveChanges();
            MessageBox.Show("Request submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}