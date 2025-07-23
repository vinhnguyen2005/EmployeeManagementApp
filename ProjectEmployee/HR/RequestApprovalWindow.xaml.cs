using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProjectEmployee.Services;

namespace ProjectEmployee.HR
{
    public class HRRequestViewModel
    {
        public int RequestId { get; set; }
        public string RequestType { get; set; }
        public string OriginatorName { get; set; }
        public string EmployeeName { get; set; }
        public string Description { get; set; }
        public decimal? RaiseAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public partial class RequestApprovalWindow : Window
    {
        private readonly ApContext _context;
        private readonly User _currentUser;

        public RequestApprovalWindow(User loggedInUser)
        {
            InitializeComponent();
            _context = new ApContext();
            _currentUser = loggedInUser;

            cbStatusFilter.SelectedIndex = 0;
            LoadRequests();
        }

        private void LoadRequests(string statusFilter = "Pending")
        {
            var hrEmployeeIds = _context.UserRoles
                            .Where(ur => ur.Role.RoleName == "HR" && ur.User.EmployeeId != null)
                            .Select(ur => (int)ur.User.EmployeeId)
                            .ToList();

            if (!hrEmployeeIds.Any())
            {
                MessageBox.Show("No HR employees found in the system to approve requests.");
                dgRequests.ItemsSource = null;
                return;
            }

            var query = _context.Requests
                .Where(r => hrEmployeeIds.Contains(r.ManagerId));

            if (statusFilter != "All")
            {
                query = query.Where(r => r.Status == statusFilter);
            }

            var requests = query
                .Include(r => r.Originator)
                .Include(r => r.Employee)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new HRRequestViewModel
                {
                    RequestId = r.RequestId,
                    RequestType = r.RequestType,
                    OriginatorName = r.Originator.FirstName + " " + r.Originator.LastName,
                    EmployeeName = r.Employee.FirstName + " " + r.Employee.LastName,
                    Description = r.Description,
                    RaiseAmount = r.RaiseAmount,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                }).ToList();

            dgRequests.ItemsSource = requests;
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not HRRequestViewModel selected) return;

            var request = _context.Requests.Find(selected.RequestId);
            if (request == null) return;
            bool actionResult = false;
            switch (request.RequestType)
            {
                case "Raise Salary":
                    actionResult = HandleRaiseSalary(request);
                    break;
                case "Request Termination":
                    actionResult = HandleTermination(request);
                    break;
                default:
                    request.Status = "Approved";
                    actionResult = true;
                    break;
            }

            if (actionResult)
            {
                _context.SaveChanges();
                MessageBox.Show("Request Approved and action executed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                AuditLogger.Log("Approve Request", _currentUser, $"Approved '{request.RequestType}' request for Employee ID: {request.EmployeeId}");
                LoadRequests((cbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Pending");
            }
        }


        private bool HandleRaiseSalary(Request request)
        {
            var employee = _context.Employees.Find(request.EmployeeId);
            if (employee != null && request.RaiseAmount.HasValue)
            {
                employee.Salary += request.RaiseAmount.Value;
                request.Status = "Approved";
                return true;
            }
            return false; 
        }

        private bool HandleTermination(Request request)
        {
            var employeeToDeactivate = _context.Employees
                .Include(emp => emp.InverseManager)
                .FirstOrDefault(emp => emp.EmployeeId == request.EmployeeId);

            if (employeeToDeactivate == null)
            {
                MessageBox.Show("Employee to be terminated not found.", "Error");
                return false;
            }
            bool isManager = employeeToDeactivate.InverseManager.Any();

            if (isManager)
            {
                var reassignWindow = new ReassignManagerWindow(employeeToDeactivate, _currentUser);
                reassignWindow.Owner = this;
                reassignWindow.ShowDialog();
            }
            else
            {
                var result = MessageBox.Show($"Are you sure you want to deactivate {employeeToDeactivate.FullName}?",
                                             "Confirm Deactivation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    employeeToDeactivate.IsActive = false;
                }
                else
                {
                    return false;
                }
            }
            request.Status = "Approved";
            return true; 
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cbStatusFilter.SelectedItem is ComboBoxItem selectedItem && this.IsLoaded)
            {
                LoadRequests(selectedItem.Content.ToString());
            }
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not HRRequestViewModel selected) return;

            var request = _context.Requests.Find(selected.RequestId);
            if (request == null)
            {
                MessageBox.Show("Request not found.");
                return;
            }
            request.Status = "Rejected";
            _context.SaveChanges();
            MessageBox.Show("Request Rejected Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadRequests((cbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Pending");
        }
    }
}
