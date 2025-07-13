using System;
using System.Windows;
using System.Windows.Controls;
using ProjectEmployee.Formatter;
using ProjectEmployee.Models;

namespace ProjectEmployee
{
    public partial class EditRequestWindow : Window
    {
        private readonly ApContext _context;
        private readonly Request _request;

        public EditRequestWindow(ApContext context, Request request)
        {
            InitializeComponent();
            _context = context;
            _request = request;

            foreach (var item in cbType.Items)
            {
                if (((ComboBoxItem)item).Content.ToString() == request.RequestType)
                {
                    cbType.SelectedItem = item;
                    break;
                }
            }
            txtDescription.Text = request.Description;

            UpdateRaiseAmountVisibility();
        }

        private void UpdateRaiseAmountVisibility()
        {
            if (cbType.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content.ToString() == "Raise Salary")
            {
                lblRaiseAmount.Visibility = Visibility.Visible;
                txtRaiseAmount.Visibility = Visibility.Visible;
                txtRaiseAmount.Text = _request.RaiseAmount.HasValue ? FormattingHelper.FormatAsInteger(_request.RaiseAmount) : string.Empty;
            }
            else
            {
                lblRaiseAmount.Visibility = Visibility.Collapsed;
                txtRaiseAmount.Visibility = Visibility.Collapsed;
                txtRaiseAmount.Text = string.Empty;
            }
        }

        private void cbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRaiseAmountVisibility();
        }

        private void txtRaiseAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbType.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content.ToString() == "Raise Salary" && !string.IsNullOrEmpty(txtRaiseAmount.Text))
            {
                if (decimal.TryParse(txtRaiseAmount.Text, out decimal raiseAmount))
                {
                    txtDescription.Text = $"Requesting salary raise. Proposed raise amount: {raiseAmount}.";
                }
            }
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cbType.SelectedItem == null || string.IsNullOrEmpty(txtDescription.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            _request.RequestType = ((ComboBoxItem)cbType.SelectedItem).Content.ToString();
            _request.Description = txtDescription.Text;
            if (_request.RequestType == "Raise Salary")
            {
                if (!decimal.TryParse(txtRaiseAmount.Text, out decimal raiseAmount) || raiseAmount <= 0)
                {
                    MessageBox.Show("Please enter a valid positive raise amount.");
                    return;
                }
                if (string.IsNullOrEmpty(txtRaiseAmount.Text))
                {
                    MessageBox.Show("Please enter the raise amount.");
                    return;
                }
                _request.RaiseAmount = raiseAmount;
            }
            else
            {
                _request.RaiseAmount = null;
            }
            _context.SaveChanges();
            MessageBox.Show("Request updated successfully.");
            this.Close();
        }
    }
}