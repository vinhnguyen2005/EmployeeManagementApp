using Microsoft.Win32;
using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ProjectEmployee.EmployeeSurbodinate
{
    public partial class MyProfileWindow : Window
    {
        private readonly ApContext _context;
        private readonly Employee _currentEmployee;

        public MyProfileWindow(User currentUser)
        {
            InitializeComponent();
            _context = new ApContext();

            // Kiểm tra an toàn đầu vào
            if (currentUser?.EmployeeId == null)
            {
                MessageBox.Show("Cannot load profile. Invalid user data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            try
            {
                // Tải thông tin employee đầy đủ nhất từ database
                _currentEmployee = _context.Employees
                    .Include(e => e.Job)
                    .Include(e => e.Department)
                        .ThenInclude(d => d.Location)
                            .ThenInclude(l => l.Country)
                    .Include(e => e.Manager)
                    .Include(e => e.Dependents)
                    .FirstOrDefault(e => e.EmployeeId == currentUser.EmployeeId);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while fetching data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void LoadData()
        {
            if (_currentEmployee == null)
            {
                MessageBox.Show("Could not load employee data.");
                this.Close();
                return;
            }

            // --- Điền thông tin vào Header ---
            txtName.Text = _currentEmployee.FullName;
            txtJobTitle.Text = _currentEmployee.Job?.JobTitle ?? "N/A";
            DisplayProfilePicture();

            // --- Điền thông tin có thể sửa ---
            txtPhoneNumber.Text = _currentEmployee.PhoneNumber;
            txtEmail.Text = _currentEmployee.Email;

            // --- Điền thông tin chỉ xem ---
            txtDepartment.Text = _currentEmployee.Department?.DepartmentName ?? "N/A";
            txtManager.Text = _currentEmployee.Manager?.FullName ?? "N/A";
            txtHireDate.Text = _currentEmployee.HireDate.ToString("MMMM dd, yyyy");

            if (_currentEmployee.Department?.Location != null)
            {
                var loc = _currentEmployee.Department.Location;
                txtLocation.Text = $"{loc.StreetAddress}, {loc.City}, {loc.StateProvince}, {loc.Country?.CountryName}";
            }
            else
            {
                txtLocation.Text = "N/A";
            }

            LoadDependents();
        }

        private void DisplayProfilePicture()
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentEmployee.ProfilePicturePath) && File.Exists(_currentEmployee.ProfilePicturePath))
                {
                    ProfileImageBrush.ImageSource = new BitmapImage(new Uri(_currentEmployee.ProfilePicturePath, UriKind.Absolute));
                }
                else
                {
                    ProfileImageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/Shinosuke.jpg"));
                }
            }
            catch
            {
                ProfileImageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/Shinosuke.jpg"));
            }
        }

        private void ChangePicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string avatarsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Avatars");
                    Directory.CreateDirectory(avatarsFolderPath);

                    string fileExtension = Path.GetExtension(openFileDialog.FileName);
                    string newFileName = $"{Guid.NewGuid()}{fileExtension}";
                    string destinationPath = Path.Combine(avatarsFolderPath, newFileName);
                    File.Copy(openFileDialog.FileName, destinationPath);
                    _currentEmployee.ProfilePicturePath = destinationPath;
                    DisplayProfilePicture();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                }
            }
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _currentEmployee.PhoneNumber = txtPhoneNumber.Text;
            _currentEmployee.Email = txtEmail.Text;

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Profile updated successfully!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save changes: {ex.Message}");
            }
        }

        private void LoadDependents()
        {
            _context.Entry(_currentEmployee).Collection(e => e.Dependents).Load();
            lvDependents.ItemsSource = _currentEmployee.Dependents.ToList();
        }

        private void AddDependent_Click(object sender, RoutedEventArgs e)
        {
            var dependentWindow = new AddEditDependentWindow(null, _currentEmployee.EmployeeId);
            if (dependentWindow.ShowDialog() == true) { LoadDependents(); }
            MessageBox.Show("Add Dependent window will open here.");
        }

        private void EditDependent_Click(object sender, RoutedEventArgs e)
        {
            if (lvDependents.SelectedItem is Dependent selectedDependent)
            {
                var dependentWindow = new AddEditDependentWindow(selectedDependent, _currentEmployee.EmployeeId);
                if (dependentWindow.ShowDialog() == true) { LoadDependents(); }
                MessageBox.Show($"Editing dependent: {selectedDependent.FirstName}");
            }
            else
            {
                MessageBox.Show("Please select a dependent to edit.");
            }
        }

        private void DeleteDependent_Click(object sender, RoutedEventArgs e)
        {
            if (lvDependents.SelectedItem is Dependent selectedDependent)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selectedDependent.FirstName}?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Dependents.Remove(selectedDependent);
                    _context.SaveChanges();
                    LoadDependents();
                }
            }
            else
            {
                MessageBox.Show("Please select a dependent to delete.");
            }
        }
    }
}
