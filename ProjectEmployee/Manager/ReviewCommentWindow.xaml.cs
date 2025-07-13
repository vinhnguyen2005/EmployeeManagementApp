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
    /// Interaction logic for ReviewCommentWindow.xaml
    /// </summary>
    public partial class ReviewCommentWindow : Window
    {
        private readonly TaskAssign _task;
        private readonly ApContext _context;

        public ReviewCommentWindow(TaskAssign task, ApContext context)
        {
            InitializeComponent();
            _task = task;
            _context = context;
            txtComment.Text = _task.ReviewComment;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _task.ReviewComment = txtComment.Text.Trim();
            _context.SaveChanges();
            MessageBox.Show("Review comment saved.");
            Close();
        }
    }
}
