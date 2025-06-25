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
    /// Interaction logic for GradePerformanceWindow.xaml
    /// </summary>
    public partial class GradePerformanceWindow : Window
    {
        private readonly TaskAssign _task;
        private readonly ApContext _context;

        public GradePerformanceWindow(TaskAssign task, ApContext context)
        {
            InitializeComponent();
            _context = context;
            _task = task;
            cbScore.SelectedIndex = (_task.PerformanceScore.HasValue ? (int)_task.PerformanceScore.Value - 1 : -1);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (cbScore.SelectedItem is ComboBoxItem item && int.TryParse(item.Content.ToString(), out int score))
            {
                _task.PerformanceScore = score;
                _context.SaveChanges();
                MessageBox.Show("Performance score saved.");
                Close();
            }
            else
            {
                MessageBox.Show("Please select a valid score.");
            }
        }
    }
}
