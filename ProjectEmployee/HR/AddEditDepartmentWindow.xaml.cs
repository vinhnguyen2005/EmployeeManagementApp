using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectEmployee.HR
{
    public partial class AddEditDepartmentWindow : Window
    {
        private readonly ApContext _context;
        private readonly Department _department;

        public AddEditDepartmentWindow(Department department)
        {
            InitializeComponent();
            _context = new ApContext();

            PopulateCountries();

            if (department == null)
            {
                _department = new Department();
                txtTitle.Text = "Add New Department";
            }
            else
            {
                _department = _context.Departments.Find(department.DepartmentId);
                txtTitle.Text = "Edit Department";
                LoadData();
            }
        }

        private void PopulateCountries()
        {
            cboCountry.ItemsSource = _context.Countries.OrderBy(c => c.CountryName).ToList();
            cboCountry.DisplayMemberPath = "CountryName";
            cboCountry.SelectedValuePath = "CountryId";
        }

        private void CboCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboCountry.SelectedValue is string countryId)
            {
                var locationsInCountry = _context.Locations
                    .Where(l => l.CountryId == countryId)
                    .OrderBy(l => l.City)
                    .ToList();

                cboLocation.ItemsSource = locationsInCountry;
                cboLocation.DisplayMemberPath = "FullAddress"; 
                cboLocation.SelectedValuePath = "LocationId";
            }
        }

        private void LoadData()
        {
            txtName.Text = _department.DepartmentName;
            var location = _context.Locations.Find(_department.LocationId);
            if (location != null)
            {
                cboCountry.SelectedValue = location.CountryId;
                cboLocation.SelectedValue = location.LocationId;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || cboLocation.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error");
                return;
            }

            _department.DepartmentName = txtName.Text;
            _department.LocationId = (int)cboLocation.SelectedValue;

            if (_department.DepartmentId == 0)
            {
                _context.Departments.Add(_department);
            }

            _context.SaveChanges();
            MessageBox.Show("Department saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }
    }
}
