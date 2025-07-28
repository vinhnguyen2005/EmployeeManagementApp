using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using ProjectEmployee.Models;

namespace ProjectEmployee
{
    public partial class ForgotPasswordWindow : Window
    {
        private readonly ApContext _context;

        public ForgotPasswordWindow()
        {
            InitializeComponent();
            _context = new ApContext();
        }

        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtNewPasswordVisible.Text = txtNewPassword.Password;
            txtConfirmPasswordVisible.Text = txtConfirmPassword.Password;
            
            txtNewPassword.Visibility = Visibility.Collapsed;
            txtConfirmPassword.Visibility = Visibility.Collapsed;
            txtNewPasswordVisible.Visibility = Visibility.Visible;
            txtConfirmPasswordVisible.Visibility = Visibility.Visible;
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtNewPassword.Password = txtNewPasswordVisible.Text;
            txtConfirmPassword.Password = txtConfirmPasswordVisible.Text;
            
            txtNewPasswordVisible.Visibility = Visibility.Collapsed;
            txtConfirmPasswordVisible.Visibility = Visibility.Collapsed;
            txtNewPassword.Visibility = Visibility.Visible;
            txtConfirmPassword.Visibility = Visibility.Visible;
        }

        private void txtNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtNewPasswordVisible.Visibility == Visibility.Visible)
                txtNewPasswordVisible.Text = txtNewPassword.Password;
        }

        private void txtConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtConfirmPasswordVisible.Visibility == Visibility.Visible)
                txtConfirmPasswordVisible.Text = txtConfirmPassword.Password;
        }

        private void txtNewPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNewPassword.Visibility == Visibility.Collapsed)
                txtNewPassword.Password = txtNewPasswordVisible.Text;
        }

        private void txtConfirmPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtConfirmPassword.Visibility == Visibility.Collapsed)
                txtConfirmPassword.Password = txtConfirmPasswordVisible.Text;
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtMessage.Text = "";
                txtMessage.Foreground = System.Windows.Media.Brushes.Red;
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    ShowMessage("Please enter username.", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmployeeId.Text) || !int.TryParse(txtEmployeeId.Text, out int employeeId))
                {
                    ShowMessage("Please enter a valid Employee ID.", false);
                    return;
                }

                string newPassword = txtNewPassword.Visibility == Visibility.Visible ? 
                                   txtNewPassword.Password : txtNewPasswordVisible.Text;
                string confirmPassword = txtConfirmPassword.Visibility == Visibility.Visible ? 
                                       txtConfirmPassword.Password : txtConfirmPasswordVisible.Text;

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    ShowMessage("Please enter new password.", false);
                    return;
                }

                if (newPassword.Length < 6)
                {
                    ShowMessage("Password must be at least 6 characters long.", false);
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    ShowMessage("Passwords do not match.", false);
                    return;
                }

                var user = _context.Users
                    .Include(u => u.Employee)
                    .FirstOrDefault(u => u.Username.ToLower() == txtUsername.Text.ToLower() && 
                                   u.EmployeeId == employeeId);

                if (user == null)
                {
                    ShowMessage("Invalid username or Employee ID.", false);
                    return;
                }

                if (user.IsLocked)
                {
                    ShowMessage("This account is locked. Please contact administrator.", false);
                    return;
                }

                user.PasswordHash = newPassword; 
                _context.SaveChanges();

                ShowMessage("Password reset successfully! You can now login with your new password.", true);

                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    this.DialogResult = true;
                    this.Close();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error: {ex.Message}", false);
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            txtMessage.Text = message;
            txtMessage.Foreground = isSuccess ? 
                System.Windows.Media.Brushes.Green : 
                System.Windows.Media.Brushes.Red;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}