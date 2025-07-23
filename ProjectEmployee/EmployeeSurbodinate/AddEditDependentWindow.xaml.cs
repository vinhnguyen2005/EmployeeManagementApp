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
    /// Interaction logic for AddEditDependentWindow.xaml
    /// </summary>
    public partial class AddEditDependentWindow : Window
    {
        private readonly ApContext _context;
        private readonly Dependent _dependent;
        private readonly int _employeeId;
        public AddEditDependentWindow(Dependent dependent, int employeeId)
        {
            InitializeComponent();
            _context = new ApContext();
            _employeeId = employeeId;

            if (dependent == null)
            {
                _dependent = new Dependent { EmployeeId = _employeeId };
                txtTitle.Text = "Add New Dependent";
            }
            else
            {
                _dependent = _context.Dependents.Find(dependent.DependentId);
                txtTitle.Text = "Edit Dependent";
                LoadData();
            }
        }
        private void LoadData()
        {
            txtFirstName.Text = _dependent.FirstName;
            txtLastName.Text = _dependent.LastName;
            cboRelationship.Text = _dependent.Relationship; 
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                cboRelationship.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _dependent.FirstName = txtFirstName.Text;
            _dependent.LastName = txtLastName.Text;
            _dependent.Relationship = (cboRelationship.SelectedItem as ComboBoxItem).Content.ToString();
            try
            {
                if (_dependent.DependentId == 0)
                {
                    _context.Dependents.Add(_dependent);
                }
                else 
                {
                    _context.Dependents.Update(_dependent);
                }

                _context.SaveChanges();
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error");
            }
        }
    }

}
