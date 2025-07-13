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
using ProjectEmployee.EmployeeSurbodinate;
using ProjectEmployee.HR;
using ProjectEmployee.Models;

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

            // Dùng 'using' để tạo một context tạm thời CHỈ để xác thực
            using var authContext = new ApContext();

            try
            {
                var user = authContext.Users
                    .Include(u => u.Employee)
                        .ThenInclude(emp => emp.Job) // Tải sẵn Job và Department
                    .Include(u => u.Employee)
                        .ThenInclude(emp => emp.Department)
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefault(u => u.Username == username);

                if (user == null || user.PasswordHash != password)
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show($"Welcome, {user.Username}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                var userRoles = user.UserRoles.Select(ur => ur.Role.RoleName.ToUpper()).ToList();

                // SỬA LỖI: Không truyền 'context' vào constructor của các Dashboard nữa
                if (userRoles.Contains("HR"))
                {
                    var hrDashboard = new HRDashboard(user); // Chỉ truyền user
                    hrDashboard.Show();
                    this.Close();
                }
                else if (userRoles.Contains("MANAGER"))
                {
                    var managerDashboard = new ManagerDashboard(user); // Chỉ truyền user
                    managerDashboard.Show();
                    this.Close();
                }
                else if (userRoles.Contains("EMPLOYEE"))
                {
                    var employeeDashboard = new EmployeeDashboard(user); // Chỉ truyền user
                    employeeDashboard.Show();
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
