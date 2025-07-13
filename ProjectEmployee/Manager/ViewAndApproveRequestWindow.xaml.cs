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

namespace ProjectEmployee.Manager
{
    /// <summary>
    /// Interaction logic for ViewAndApproveRequestWindow.xaml
    /// </summary>
    public partial class ViewAndApproveRequestWindow : Window
    {
        private readonly ApContext _context;
        private readonly int _employeeId; 
        private User _currentUser;

        public ViewAndApproveRequestWindow(int employeeId, ApContext context, User loggedInUser)
        {
            InitializeComponent();
            _context = context;
            _currentUser = loggedInUser;
            _employeeId = employeeId;
            LoadAllPendingRequests();
        }

        private void LoadAllPendingRequests()
        {
            try
            {
                var pendingRequests = _context.Requests
                    .Where(r => r.OriginatorId == _employeeId && r.ManagerId == _currentUser.EmployeeId && r.Status == "Pending")
                    .OrderBy(r => r.CreatedAt)
                    .ToList();
                var employee = _context.Employees.Find(_employeeId);
                if (employee != null)
                {
                    txtHeader.Text = $"Pending Requests from: {employee.FirstName} {employee.LastName}";
                }

                dgEmployeeRequests.ItemsSource = pendingRequests;
                if (!pendingRequests.Any())
                {
                    MessageBox.Show("This employee has no more pending requests.", "All Done", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading requests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Request requestToProcess)
            {
                var requestInDb = _context.Requests.Find(requestToProcess.RequestId);
                if (requestInDb != null)
                {
                    requestInDb.Status = "Approved";
                    _context.SaveChanges();
                    LoadAllPendingRequests();
                }
            }
        }
        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Request requestToProcess)
            {
                var requestInDb = _context.Requests.Find(requestToProcess.RequestId);
                if (requestInDb != null)
                {
                    requestInDb.Status = "Rejected";
                    _context.SaveChanges();
                    LoadAllPendingRequests();
                }
            }
        }
    }
}
