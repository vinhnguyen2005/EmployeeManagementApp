using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ProjectEmployee.DTO;
using ProjectEmployee.NewFolder;

namespace ProjectEmployee
{
    public partial class PerformanceReviewWindow : Window
    {
        private readonly User _currentUser;
        private readonly ApContext _context;

        public PerformanceReviewWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _context = new ApContext();
            LoadPerformanceData();
        }

        private void LoadPerformanceData()
        {
            try
            {
                var teamTasks = _context.Tasks
                    .Include(t => t.Employee)
                    .Where(t => t.Employee.ManagerId == _currentUser.EmployeeId && t.Employee.IsActive == true)
                    .Select(t => new
                    {
                        t.TaskId,
                        t.TaskDescription,
                        t.Status,
                        EmployeeId = t.Employee.EmployeeId,
                        EmployeeName = t.Employee.FirstName + " " + t.Employee.LastName,
                        PerformanceScore = t.PerformanceScore ?? 0,
                        ReviewComment = t.ReviewComment ?? "",
                        t.Deadline,
                        t.CompletedDate,
                    })
                    .OrderBy(t => t.EmployeeName)
                    .ThenBy(t => t.TaskId)
                    .ToList();

                // Nếu không có task nào, dọn dẹp giao diện và thoát ra sớm
                if (!teamTasks.Any())
                {
                    dgPerformance.ItemsSource = null;
                    UpdateStatistics(0, new Dictionary<int, EmployeeAverageViewModel>());
                    return;
                }

                var employeeAverages = teamTasks
                    .GroupBy(t => new { t.EmployeeId, t.EmployeeName })
                    .ToDictionary(
                        g => g.Key.EmployeeId,
                        g => new EmployeeAverageViewModel
                        {
                            EmployeeName = g.Key.EmployeeName,
                            TaskCount = g.Count(),
                            AvgScore = g.Any() ? g.Average(x => x.PerformanceScore) : 0
                        });

                // SỬA LỖI: Thêm () và {} sau PerformanceReviewViewModel
                var detailedPerformanceData = new List<PerformanceReviewViewModel>();

                foreach (var task in teamTasks)
                {
                    if (employeeAverages.TryGetValue(task.EmployeeId, out var employeeAvg))
                    {
                        var isFirstTask = !detailedPerformanceData.Any(x => x.EmployeeId == task.EmployeeId);

                        // SỬA LỖI: Xử lý an toàn cho PerformanceScore
                        var safePerformanceScore = task.PerformanceScore;
                        var safeAvgScore = employeeAvg?.AvgScore ?? 0;

                        // Đảm bảo giá trị không null và hợp lệ
                        decimal taskScore = 0;
                        double taskScoreDouble = 0;
                        double avgScoreDouble = 0;

                        try
                        {
                            taskScore = Convert.ToDecimal(safePerformanceScore);
                            taskScoreDouble = Convert.ToDouble(safePerformanceScore);
                            avgScoreDouble = Convert.ToDouble(safeAvgScore);
                        }
                        catch
                        {
 
                            taskScore = 0;
                            taskScoreDouble = 0;
                            avgScoreDouble = 0;
                        }

                        detailedPerformanceData.Add(new PerformanceReviewViewModel
                        {
                            EmployeeId = task.EmployeeId,
                            EmployeeInfo = isFirstTask ? $"{employeeAvg.EmployeeName} ({employeeAvg.TaskCount} tasks)" : "",
                            TaskId = task.TaskId,
                            TaskTitle = task.TaskDescription ?? "Untitled Task",
                            TaskStatus = GetStatusText(task.Status, task.Deadline, task.CompletedDate),
                            TaskComment = task.ReviewComment,
                            Deadline = task.Deadline,
                            CompletedDate = task.CompletedDate,
                            CompletionInfo = task.Status.ToLower() == "completed" && task.CompletedDate.HasValue
                                ? $"Finished: {task.CompletedDate.Value:dd/MM/yyyy}"
                                : "",
                            TaskScore = taskScore,
                            TaskScoreLabel = GetLabel(taskScoreDouble),
                            TaskScoreColor = GetColor(taskScoreDouble),
                            AvgScore = safeAvgScore,
                            AvgScoreLabel = GetLabel(avgScoreDouble),
                            AvgScoreColor = GetColor(avgScoreDouble),
                            IsFirstTaskForEmployee = isFirstTask
                        });
                    }
                }

                dgPerformance.ItemsSource = detailedPerformanceData;
                UpdateStatistics(teamTasks.Count, employeeAverages);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.InnerException?.Message ?? ex.Message}", "Error");
            }
        }

        // SỬA LỖI: Thêm kiểm tra null và empty cho averages
        private void UpdateStatistics(int totalTasks, Dictionary<int, EmployeeAverageViewModel> averages)
        {
            int totalEmployees = _context.Employees
                .Count(e => e.ManagerId == _currentUser.EmployeeId && e.IsActive == true);

            txtTotalEmployees.Text = totalEmployees.ToString();
            txtTotalTasks.Text = totalTasks.ToString();

            // SỬA LỖI: Kiểm tra null và empty trước khi tính Average
            if (averages != null && averages.Any())
            {
                // Thêm kiểm tra để tránh lỗi khi tính Average
                var avgScores = averages.Values.Where(x => x.AvgScore > 0).ToList();
                if (avgScores.Any())
                {
                    txtAvgPerformance.Text = avgScores.Average(x => x.AvgScore).ToString("0.00");
                }
                else
                {
                    txtAvgPerformance.Text = "0.00";
                }

                var topPerformers = averages.Values
                    .Where(x => x.AvgScore >= 4.5m)
                    .Select(x => x.EmployeeName)
                    .ToList();

                txtTopPerformers.Text = topPerformers.Any() ? string.Join(", ", topPerformers) : "No top performers";
            }
            else // Nếu không có dữ liệu, hiển thị giá trị mặc định
            {
                txtAvgPerformance.Text = "0.00";
                txtTopPerformers.Text = "N/A";
            }
        }

        private string GetStatusText(string status, DateTime deadline, DateTime? completedDate)
        {
            if (status.ToLower() == "completed")
            {
                if (completedDate.HasValue && deadline.Date < completedDate.Value.Date)
                {
                    return "✅ Completed (Late)";
                }
                return "✅ Completed";
            }
            string overdueLabel = "";
            if (deadline.Date < DateTime.Today.Date)
            {
                overdueLabel = " (Overdue)";
            }
            string statusText = status.ToLower() switch
            {
                "in progress" => "🔄 In Progress",
                "pending" => "⏳ Pending",
                _ => "📋 Unknown"
            };
            return statusText + overdueLabel;
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

            // SỬA LỖI: Sử dụng FirstOrDefault để tránh lỗi khi không tìm thấy
            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == selected.TaskId);
            if (task == null)
            {
                MessageBox.Show("Task not found.");
                return;
            }

            if (task.Status.ToLower() != "completed")
            {
                MessageBox.Show("You can only grade completed tasks.");
                return;
            }

            var gradeWindow = new GradePerformanceWindow(task, _context);
            gradeWindow.Owner = this;
            gradeWindow.ShowDialog();
            LoadPerformanceData();
        }

        private void AddReviewComment_Click(object sender, RoutedEventArgs e)
        {
            if (dgPerformance.SelectedItem is not PerformanceReviewViewModel selected)
            {
                MessageBox.Show("Please select a task to comment.");
                return;
            }

            // SỬA LỖI: Kiểm tra null ngay sau khi query
            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == selected.TaskId);
            if (task == null)
            {
                MessageBox.Show("Task not found.");
                return;
            }

            var commentWindow = new ReviewCommentWindow(task, _context);
            commentWindow.Owner = this;
            commentWindow.ShowDialog();
            LoadPerformanceData();
        }
    }
}