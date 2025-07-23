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

namespace ProjectEmployee.HR
{
    /// <summary>
    /// Interaction logic for AddEditJobWindow.xaml
    /// </summary>
    public partial class AddEditJobWindow : Window
    {
        private readonly ApContext _context;
        private readonly Job _job;
        public AddEditJobWindow(Job job)
        {
            InitializeComponent();
            _context = new ApContext();

            if (job == null)
            {
                _job = new Job();
                txtTitle.Text = "Add New Job";
            }
            else
            {
                _job = _context.Jobs.Find(job.JobId);
                txtTitle.Text = "Edit Job";
                LoadData();
            }
        }

        private void LoadData()
        {
            txtJobTitle.Text = _job.JobTitle;
            txtMinSalary.Text = _job.MinSalary?.ToString();
            txtMaxSalary.Text = _job.MaxSalary?.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtJobTitle.Text))
            {
                MessageBox.Show("Job Title is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal? minSalary = null;
            decimal? maxSalary = null;

            if (!string.IsNullOrWhiteSpace(txtMinSalary.Text))
            {
                if (!decimal.TryParse(txtMinSalary.Text, out decimal min) || min <= 0)
                {
                    MessageBox.Show("Minimum Salary must be a valid", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                minSalary = min;
            }

            if (!string.IsNullOrWhiteSpace(txtMaxSalary.Text))
            {
                if (!decimal.TryParse(txtMaxSalary.Text, out decimal max) || max <= 0)
                {
                    MessageBox.Show("Maximum Salary must be a valid.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                maxSalary = max;
            }

            if (minSalary.HasValue && maxSalary.HasValue && minSalary > maxSalary)
            {
                MessageBox.Show("Minimum Salary cannot be greater than Maximum Salary.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _job.JobTitle = txtJobTitle.Text;
            _job.MinSalary = minSalary;
            _job.MaxSalary = maxSalary;

            try
            {
                if (_job.JobId == 0)
                {
                    _context.Jobs.Add(_job);
                }
                else 
                {
                    _context.Jobs.Update(_job);
                }

                _context.SaveChanges();
                MessageBox.Show("Job saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving: {ex.Message}", "Error");
            }
        }
    }
}
