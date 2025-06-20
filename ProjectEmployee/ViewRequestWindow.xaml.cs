using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProjectEmployee.Formatter;
using ProjectEmployee.Models;

namespace ProjectEmployee
{
    public partial class ViewRequestWindow : Window
    {
        private readonly ApContext _context;
        private readonly User _currentUser;
        private Request _selectedRequest;
        private bool isDataLoaded = false;
        private int selectedIndex = -1;

        public ViewRequestWindow(ApContext context, User currentUser)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (cbStatusFilter.SelectedIndex == -1)
            {
                cbStatusFilter.SelectedIndex = 0;
            }
            LoadRequests("All");
        }

        private void dgRequests_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isDataLoaded && (dgRequests.ItemsSource == null || dgRequests.Items.Count == 0))
            {
                LoadRequests((cbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All");
            }
        }

        private void LoadRequests(string statusFilter = "All")
        {
            try
            {
                var query = _context.Requests
                    .Where(r => r.ManagerId == _currentUser.EmployeeId);

                if (statusFilter != "All")
                {
                    query = query.Where(r => r.Status == statusFilter);
                }

                var result = query
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new RequestViewModel
                    {
                        RequestId = r.RequestId,
                        RequestType = r.RequestType,
                        Description = r.Description,
                        Status = r.Status,
                        CreatedAt = r.CreatedAt,
                        RaiseAmountFormatted = FormattingHelper.FormatAsDollarInteger(r.RaiseAmount)
                    })
                    .ToList();

                if (dgRequests == null)
                {
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    dgRequests.ItemsSource = null;
                    dgRequests.ItemsSource = result;
                    dgRequests.Items.Refresh();
                    isDataLoaded = true;
                    if (selectedIndex >= 0 && selectedIndex < dgRequests.Items.Count)
                    {
                        dgRequests.SelectedIndex = selectedIndex;
                        var selectedItem = dgRequests.Items[selectedIndex] as RequestViewModel;
                        if (selectedItem != null)
                        {
                            _selectedRequest = _context.Requests.FirstOrDefault(r => r.RequestId == selectedItem.RequestId);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading requests: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        private void dgRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIndex = dgRequests.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < dgRequests.Items.Count)
            {
                var selectedItem = dgRequests.Items[selectedIndex] as RequestViewModel;
                if (selectedItem != null)
                {
                    _selectedRequest = _context.Requests.FirstOrDefault(r => r.RequestId == selectedItem.RequestId);
                }
            }
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cbStatusFilter.SelectedItem is ComboBoxItem selectedItem)
            {
                string status = selectedItem.Content.ToString();
                LoadRequests(status);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RevertRequest_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRequest == null && selectedIndex >= 0 && selectedIndex < dgRequests.Items.Count)
            {
                var selectedItem = dgRequests.Items[selectedIndex] as RequestViewModel;
                if (selectedItem != null)
                {
                    _selectedRequest = _context.Requests.FirstOrDefault(r => r.RequestId == selectedItem.RequestId);
                }
            }
            if (_selectedRequest == null)
            {
                MessageBox.Show("Please select a request to revert.");
                return;
            }
            if (_selectedRequest.Status != RequestStatus.Cancelled)
            {
                MessageBox.Show("Only cancelled requests can be reverted.");
                return;
            }
            _selectedRequest.Status = RequestStatus.Pending;
            _context.SaveChanges();
            LoadRequests((cbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All");
        }

        private void CancelRequest_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRequest == null && selectedIndex >= 0 && selectedIndex < dgRequests.Items.Count)
            {
                var selectedItem = dgRequests.Items[selectedIndex] as RequestViewModel;
                if (selectedItem != null)
                {
                    _selectedRequest = _context.Requests.FirstOrDefault(r => r.RequestId == selectedItem.RequestId);
                }
            }
            if (_selectedRequest == null)
            {
                MessageBox.Show("Please select a request to cancel.");
                return;
            }
            if (_selectedRequest.Status != RequestStatus.Pending)
            {
                MessageBox.Show("Only pending requests can be cancelled.");
                return;
            }
            _selectedRequest.Status = RequestStatus.Cancelled;
            _context.SaveChanges();
            LoadRequests((cbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All");
        }

        private void EditRequest_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRequest == null && selectedIndex >= 0 && selectedIndex < dgRequests.Items.Count)
            {
                var selectedItem = dgRequests.Items[selectedIndex] as RequestViewModel;
                if (selectedItem != null)
                {
                    _selectedRequest = _context.Requests.FirstOrDefault(r => r.RequestId == selectedItem.RequestId);
                }
            }
            if (_selectedRequest == null)
            {
                MessageBox.Show("Please select a request to edit.");
                return;
            }
            MessageBox.Show($"Attempting to edit: {_selectedRequest.RequestType}, Status: {_selectedRequest.Status}, Raise Amount: {FormattingHelper.FormatAsDollarInteger(_selectedRequest.RaiseAmount)}");
            if (_selectedRequest.Status == RequestStatus.Approved)
            {
                MessageBox.Show("Approved requests cannot be edited.");
                return;
            }
            var editWindow = new EditRequestWindow(_context, _selectedRequest);
            editWindow.ShowDialog();
            LoadRequests((cbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All");
        }
    }

    // Thêm lớp ViewModel để tránh dynamic
    public class RequestViewModel
    {
        public int RequestId { get; set; }
        public string RequestType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RaiseAmountFormatted { get; set; }
    }
}