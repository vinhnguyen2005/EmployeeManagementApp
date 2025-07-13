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
    /// Interaction logic for SubmitRequestEmployeeWindow.xaml
    /// </summary>
    public partial class SubmitRequestEmployeeWindow : Window
    {
        private readonly User _currentUser;
        private readonly ApContext _context;
        private readonly Employee _currentEmployee;
        public SubmitRequestEmployeeWindow(User loggedInUser)
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

        }

        private void CbRequestType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRequestType.SelectedItem is ComboBoxItem selectedItem)
            {
                string type = selectedItem.Content.ToString();
                if (type == "Leave" || type == "Work From Home")
                {
                    pnlDatePickers.Visibility = Visibility.Visible;
                    pnlOtherType.Visibility = Visibility.Collapsed;
                }
                else if (type == "Other")
                {
                    pnlDatePickers.Visibility = Visibility.Collapsed;
                    pnlOtherType.Visibility = Visibility.Visible;
                }
                else
                {
                    pnlDatePickers.Visibility = Visibility.Collapsed;
                    pnlOtherType.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (cbRequestType.SelectedItem == null)
            {
                MessageBox.Show("Please select a request type.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Please enter a description for your request.");
                return;
            }
            var newRequest = new Request
            {
                OriginatorId = _currentEmployee.EmployeeId,
                EmployeeId = _currentEmployee.EmployeeId,
                ManagerId = (int)_currentEmployee.ManagerId,
                RequestType = (cbRequestType.SelectedItem as ComboBoxItem).Content.ToString(),
                Description = txtDescription.Text.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            if (pnlDatePickers.Visibility == Visibility.Visible)
            {
                if (dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
                {
                    MessageBox.Show("Please select both start and end dates.");
                    return;
                }
                newRequest.StartDate = dpStartDate.SelectedDate.Value;
                newRequest.EndDate = dpEndDate.SelectedDate.Value;
            }
            try
            {
                _context.Requests.Add(newRequest);
                _context.SaveChanges();
                MessageBox.Show("Request submitted successfully.");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
