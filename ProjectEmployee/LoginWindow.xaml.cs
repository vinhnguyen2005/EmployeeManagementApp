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
using Microsoft.EntityFrameworkCore;
using ProjectEmployee.Admin;
using ProjectEmployee.EmployeeSurbodinate;
using ProjectEmployee.HR;
using ProjectEmployee.Models;
using ProjectEmployee.Services;

namespace ProjectEmployee
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        ApContext context = new ApContext();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var context = new ApContext();

            try
            {
                var user = context.Users
                    .Include(u => u.Employee) 
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefault(u => u.Username == username);

                if (user == null || user.PasswordHash != password)
                {
                    AuditLogger.Log("Login Failed", null, $"Failed login attempt for username: '{username}'.");
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (user.IsLocked)
                {
                    MessageBox.Show("Your account has been locked. Please contact the administrator.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (user.Employee != null && !user.Employee.IsActive)
                {
                    MessageBox.Show("Your employee profile is inactive. Please contact HR.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                AuditLogger.Log("User Login", user, $"User '{user.Username}' logged in successfully.");
                MessageBox.Show($"Welcome, {user.Username}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                var userRoles = user.UserRoles.Select(ur => ur.Role.RoleName.ToUpper()).ToList();

                if (userRoles.Contains("ADMIN"))
                {
                    new AdminDashboard(user).Show();
                    this.Close();
                }
                else if (userRoles.Contains("HR"))
                {
                    new HR.HRDashboard(user).Show();
                    this.Close();
                }
                else if (userRoles.Contains("MANAGER"))
                {
                    new ManagerDashboard(user).Show();
                    this.Close();
                }
                else if (userRoles.Contains("EMPLOYEE"))
                {
                    new EmployeeSurbodinate.EmployeeDashboard(user).Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("User has no assigned roles.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPasswordVisible.Visibility = Visibility.Visible;
            txtPassword.Visibility = Visibility.Collapsed;
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtPasswordVisible.Visibility == Visibility.Visible)
                txtPasswordVisible.Text = txtPassword.Password;
        }

        private void txtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPassword.Visibility == Visibility.Visible)
                txtPassword.Password = txtPasswordVisible.Text;
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Please contact your administrator to reset your password.", "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
