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
using Microsoft.IdentityModel.Tokens;
using ProjectEmployee.Models;

namespace ProjectEmployee.EmployeeSurbodinate
{
    /// <summary>
    /// Interaction logic for MyTasksWindow.xaml
    /// </summary>
    public partial class MyTasksWindow : Window
    {
        private readonly ApContext _context;
        private readonly User _currentUser;
        private readonly Employee _currentEmployee;

        public MyTasksWindow(User loggedInUser)
        {
            InitializeComponent();
            _context = new ApContext();
            _currentUser = loggedInUser;
            _currentEmployee = loggedInUser.Employee;

            if (_currentEmployee == null)
            {
                MessageBox.Show("User is not linked to an employee record.");
                Close();
                return;
            }

            cbStatusFilter.SelectedIndex = 0; 
            LoadTasks();
        }

        private void LoadTasks(string statusFilter = "All")
        {
            var query = _context.Tasks
                .Where(t => t.EmployeeId == _currentEmployee.EmployeeId);

            if (statusFilter != "All")
            {
                query = query.Where(t => t.Status == statusFilter);
            }
            dgMyTasks.ItemsSource = query.OrderBy(t => t.Deadline).ToList();
            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            var allTasks = _context.Tasks
                .Where(t => t.EmployeeId == _currentEmployee.EmployeeId)
                .ToList();

            txtPendingCount.Text = allTasks.Count(t => t.Status == "Pending").ToString();
            txtInProgressCount.Text = allTasks.Count(t => t.Status == "In Progress").ToString();
            txtCompletedCount.Text = allTasks.Count(t => t.Status == "Completed").ToString();
            txtOverdueCount.Text = allTasks.Count(t => t.Deadline < DateTime.Today && t.Status != "Completed").ToString();
        }

        private void SetInProgress_Click(object sender, RoutedEventArgs e)
        {
            if (dgMyTasks.SelectedItem is TaskAssign selectedTask)
            {
                var taskToUpdate = _context.Tasks.Find(selectedTask.TaskId);
                if (taskToUpdate != null)
                {
                    taskToUpdate.Status = "In Progress";
                    _context.SaveChanges();
                    LoadTasks(cbStatusFilter.SelectedItem is ComboBoxItem item ? item.Content.ToString() : "All");
                }
            }
            else
            {
                MessageBox.Show("Please select a task first.");
            }
        }

        private void MarkCompleted_Click(object sender, RoutedEventArgs e)
        {
            if(dgMyTasks.SelectedItem is TaskAssign selectedTask)
            {
                var taskToUpdate = _context.Tasks.Find(selectedTask.TaskId);
                if (taskToUpdate != null)
                {
                    taskToUpdate.Status = "Completed";
                    taskToUpdate.CompletedDate = DateTime.Now; 
                    _context.SaveChanges();
                    LoadTasks(cbStatusFilter.SelectedItem is ComboBoxItem item ? item.Content.ToString() : "All");
                }
            }
            else
            {
                MessageBox.Show("Please select a task first.");
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks(cbStatusFilter.SelectedItem is ComboBoxItem item ? item.Content.ToString() : "All");
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbStatusFilter.SelectedItem is ComboBoxItem selectedItem)
            {
                LoadTasks(selectedItem.Content.ToString());
            }
        }
    }
}
