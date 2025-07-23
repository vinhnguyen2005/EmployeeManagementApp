using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;

namespace ProjectEmployee.HR
{
    public partial class EmployeeProfileWindow : Window
    {
        private readonly ApContext _context;
        private readonly int _employeeId;

        public EmployeeProfileWindow(int employeeId)
        {
            InitializeComponent();
            _context = new ApContext();
            _employeeId = employeeId;
            LoadProfileData();
        }

        private void LoadProfileData()
        {
            try
            {
                var employee = _context.Employees
                    .Include(e => e.Job)
                    .Include(e => e.Department)
                        .ThenInclude(d => d.Location)
                            .ThenInclude(l => l.Country)
                    .Include(e => e.Manager)
                    .Include(e => e.Dependents)
                    .FirstOrDefault(e => e.EmployeeId == _employeeId);

                if (employee == null)
                {
                    MessageBox.Show("Employee not found.", "Error");
                    this.Close();
                    return;
                }

                txtName.Text = $"{employee.FirstName} {employee.LastName}";
                txtJobTitle.Text = employee.Job?.JobTitle ?? "N/A";

                txtEmail.Text = employee.Email;
                txtPhone.Text = employee.PhoneNumber ?? "N/A";

                txtDepartment.Text = employee.Department?.DepartmentName ?? "N/A";
                txtManager.Text = employee.Manager?.FullName ?? "N/A";
                txtHireDate.Text = employee.HireDate.ToString("MMMM dd, yyyy");
                if (employee.Department?.Location != null)
                {
                    var loc = employee.Department.Location;
                    txtLocation.Text = $"{loc.StreetAddress}, {loc.City}, {loc.StateProvince}, {loc.Country?.CountryName}";
                }
                else
                {
                    txtLocation.Text = "No location data.";
                }
                lvDependents.ItemsSource = employee.Dependents.ToList();
                DisplayProfilePicture(employee.ProfilePicturePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load profile: {ex.Message}");
            }
        }
        private void DisplayProfilePicture(string imagePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    ProfileImageBrush.ImageSource = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
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
    }
}
