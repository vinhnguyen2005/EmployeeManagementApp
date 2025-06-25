using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ProjectEmployee.Models;
using ProjectEmployee.DTO;
using ProjectEmployee.NewFolder;

namespace ProjectEmployee
{
    public partial class PerformanceReviewWindow : Window
    {
        private readonly User _currentUser;
        private readonly ApContext _context;

        public PerformanceReviewWindow(User user, ApContext context)
        {
            InitializeComponent();
            _currentUser = user;
            _context = context;
            LoadPerformanceData();
        }

        private void LoadPerformanceData()
        {
            var teamTasks = _context.Tasks
                .Where(t => t.Employee.ManagerId == _currentUser.EmployeeId)
                .Select(t => new
                {
                    t.TaskId,
                    t.TaskDescription,
                    t.Status,
                    EmployeeId = t.Employee.EmployeeId,
                    EmployeeName = t.Employee.FirstName + " " + t.Employee.LastName,
                    PerformanceScore = t.PerformanceScore ?? 0,
                    ReviewComment = t.ReviewComment ?? ""
                })
                .OrderBy(t => t.EmployeeName)
                .ThenBy(t => t.TaskId)
                .ToList();

            // Build averages as typed dictionary
            var employeeAverages = teamTasks
                .GroupBy(t => new { t.EmployeeId, t.EmployeeName })
                .ToDictionary(
                    g => g.Key.EmployeeId,
                    g => new EmployeeAverageViewModel
                    {
                        EmployeeName = g.Key.EmployeeName,
                        TaskCount = g.Count(),
                        AvgScore = (decimal)g.Average(x => x.PerformanceScore)
                    });

            // Final view model list
            var detailedPerformanceData = new List<PerformanceReviewViewModel>();

            foreach (var task in teamTasks)
            {
                var employeeAvg = employeeAverages[task.EmployeeId];
                var isFirstTask = !detailedPerformanceData.Any(x => x.EmployeeId == task.EmployeeId);

                detailedPerformanceData.Add(new PerformanceReviewViewModel
                {
                    EmployeeId = task.EmployeeId,
                    EmployeeInfo = isFirstTask ? $"{employeeAvg.EmployeeName} ({employeeAvg.TaskCount} tasks)" : "",
                    TaskId = task.TaskId,
                    TaskTitle = task.TaskDescription ?? "Untitled Task",
                    TaskStatus = GetStatusText(task.Status),
                    TaskComment = task.ReviewComment,

                    TaskScore = (decimal)task.PerformanceScore,
                    TaskScoreLabel = GetLabel((double)task.PerformanceScore),
                    TaskScoreColor = GetColor((double)task.PerformanceScore),

                    AvgScoreLabel = GetLabel((double)employeeAvg.AvgScore),
                    AvgScoreColor = GetColor((double)employeeAvg.AvgScore),

                    IsFirstTaskForEmployee = isFirstTask
                });
            }

            dgPerformance.ItemsSource = detailedPerformanceData;

            // Statistics update
            UpdateStatistics(teamTasks.Count, employeeAverages);
        }

        private void UpdateStatistics(int totalTasks, Dictionary<int, EmployeeAverageViewModel> averages)
        {
            int totalEmployees = _context.Employees
                .Count(e => e.ManagerId == _currentUser.EmployeeId);
            txtTotalEmployees.Text =totalEmployees.ToString();
            txtTotalTasks.Text = totalTasks.ToString();
            txtAvgPerformance.Text = averages.Values.Average(x => x.AvgScore).ToString("0.00");

            var topPerformers = averages.Values
                .Where(x => x.AvgScore >= 4.5m)
                .Select(x => x.EmployeeName)
                .ToList();

            txtTopPerformers.Text = topPerformers.Any()
                ? string.Join(", ", topPerformers)
                : "No top performers";
        }

        private string GetStatusText(string status)
        {
            return status switch
            {
                "pending" => "⏳ Pending",
                "in_progress" => "🔄 In Progress",
                "completed" => "✅ Completed",
                "cancelled" => "❌ Cancelled",
                _ => "📋 Unknown"
            };
        }

        private string GetLabel(double score)
        {
            return score switch
            {
                >= 4.5 => "Excellent",
                >= 3.5 => "Good",
                >= 2.5 => "Average",
                _ => "Needs Improve"
            };
        }

        private string GetColor(double score)
        {
            return score switch
            {
                >= 4.5 => "#2ECC71",
                >= 3.5 => "#3498DB",
                >= 2.5 => "#F1C40F",
                _ => "#E74C3C"
            };
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadPerformanceData();
        }

        private void GradePerformance_Click(object sender, RoutedEventArgs e)
        {
            if (dgPerformance.SelectedItem is not PerformanceReviewViewModel selected)
            {
                MessageBox.Show("Please select a task to grade.");
                return;
            }

            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == selected.TaskId);
            if (task != null)
            {
                var gradeWindow = new GradePerformanceWindow(task, _context);
                gradeWindow.Owner = this;
                gradeWindow.ShowDialog();
                LoadPerformanceData();
            }
            else
            {
                MessageBox.Show("Task not found.");
            }
        }

        private void AddReviewComment_Click(object sender, RoutedEventArgs e)
        {
            if (dgPerformance.SelectedItem is not PerformanceReviewViewModel selected)
            {
                MessageBox.Show("Please select a task to comment.");
                return;
            }

            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == selected.TaskId);
            if (task != null)
            {
                var commentWindow = new ReviewCommentWindow(task, _context);
                commentWindow.Owner = this;
                commentWindow.ShowDialog();
                LoadPerformanceData();
            }
            else
            {
                MessageBox.Show("Task not found.");
            }
        }
    }
}
