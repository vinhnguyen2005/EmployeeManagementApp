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


namespace ProjectEmployee
{
    /// <summary>
    /// Interaction logic for AssignTaskWindow.xaml
    /// </summary>
    public partial class AssignTaskWindow : Window
    {
        private readonly User _currentUser;
        private readonly ApContext _context;
        private readonly List<Employee> _employees;

        public AssignTaskWindow(User user, ApContext context, List<Employee> employees)
        {
            InitializeComponent();
            _currentUser = user;
            _context = context;
            _employees = employees.ToList(); 
            EmployeeComboBox.ItemsSource = _employees.ToList();
            PriorityComboBox.ItemsSource = new List<string>
    {
        "🟢 Low",
        "🟡 Medium",
        "🔴 High"
    };
            PriorityComboBox.SelectedIndex = 1;
        }

        private void AssignTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = EmployeeComboBox.SelectedItem as Employee;
            if (selectedEmployee == null)
            {
                MessageBox.Show("Please select an employee to assign the task.");
                return;
            }
            var taskDescription = TaskDescription.Text.Trim();
            if (string.IsNullOrEmpty(taskDescription) || string.IsNullOrWhiteSpace(taskDescription))
            {
                MessageBox.Show("Task description cannot be empty.");
                return;
            }
            var task = new TaskAssign
            {
                EmployeeId = selectedEmployee.EmployeeId,
                TaskDescription = taskDescription,
                Deadline = DeadlinePicker.SelectedDate ?? DateTime.Today,
                Priority = PriorityComboBox.SelectedItem?.ToString() ?? "Default",
                Status = "Pending",
                CreatedDate = DateTime.Now
            };
            try
            {
                _context.Tasks.Add(task);
                _context.SaveChanges();
                MessageBox.Show("Task assigned successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
