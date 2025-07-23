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

namespace ProjectEmployee.EmployeeSurbodinate
{
    /// <summary>
    /// Interaction logic for MyRequestsHistoryWindow.xaml
    /// </summary>
    public partial class MyRequestsHistoryWindow : Window
    {
        private readonly ApContext _context;
        private readonly User _currentUser;
        private readonly Employee _currentEmployee;

        public MyRequestsHistoryWindow(User loggedInUser)
        {
            InitializeComponent();
            _context = new ApContext();
            _currentUser = loggedInUser;
            _currentEmployee = loggedInUser.Employee;
            if (_currentEmployee == null)
            {
                MessageBox.Show("User is not linked to an employee record.");
                Close();
                return;
            }

            cbStatusFilter.SelectedIndex = 0;
            LoadRequests();
        }

        private void LoadRequests(string statusFilter = "All")
        {
            var query = _context.Requests 
                .Where(r => r.OriginatorId == _currentEmployee.EmployeeId && 
                        r.EmployeeId == _currentEmployee.EmployeeId);

            if (statusFilter != "All")
            {
                query = query.Where(r => r.Status == statusFilter);
            }

            dgRequests.ItemsSource = query.OrderByDescending(r => r.CreatedAt).ToList();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbStatusFilter.SelectedItem is ComboBoxItem selectedItem && this.IsLoaded)
            {
                LoadRequests(selectedItem.Content.ToString());
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRequests(cbStatusFilter.SelectedItem is ComboBoxItem item ? item.Content.ToString() : "All");
        }

        private void CreateRequest_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new SubmitRequestEmployeeWindow(_currentUser);

            if (createWindow.ShowDialog() == true)
            {
                LoadRequests(cbStatusFilter.SelectedItem is ComboBoxItem item ? item.Content.ToString() : "All");
            }
        }

        private void CancelRequest_Click(object sender, RoutedEventArgs e)
        {
            if (dgRequests.SelectedItem is Request selectedRequest)
            {
                if (selectedRequest.Status != "Pending")
                {
                    MessageBox.Show("Only pending requests can be cancelled.", "Action Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var requestToUpdate = _context.Requests.Find(selectedRequest.RequestId);
                if (requestToUpdate != null)
                {
                    requestToUpdate.Status = "Cancelled";
                    _context.SaveChanges();
                    LoadRequests(cbStatusFilter.SelectedItem is ComboBoxItem item ? item.Content.ToString() : "All");
                }
            }
            else
            {
                MessageBox.Show("Please select a request to cancel.");
            }
        }
    }
}
