using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            if (_currentUser.EmployeeId == null) return;
            var query = _context.Requests
                .Where(r => r.ManagerId == _currentUser.EmployeeId);

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
            if (request == null)
            {
                MessageBox.Show("Request not found.");
                return;
            }
            switch (request.RequestType)
            {
                case "Raise Salary":
                    HandleRaiseSalary(request);
                    break;
                case "Request Termination":
                    HandleTermination(request);
                    break;
                default:
                    request.Status = "Approved";
                    break;
            }
            _context.SaveChanges();
            MessageBox.Show("Request Approved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadRequests((cbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Pending");
        }

        private void HandleRaiseSalary(Request request)
        {
            var employee = _context.Employees.Find(request.EmployeeId);
            if (employee != null && request.RaiseAmount.HasValue)
            {
                employee.Salary += request.RaiseAmount.Value;
                request.Status = "Approved";
            }
        }

        private void HandleTermination(Request request)
        {
            var employee = _context.Employees.Find(request.EmployeeId);
            if (employee != null)
            {
                employee.IsActive = false; 
                request.Status = "Approved";
            }
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
