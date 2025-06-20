using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using ProjectEmployee.Models;

namespace ProjectEmployee
{
    public partial class StatisticWindow : Window
    {
        private readonly User _currentUser;
        private readonly ApContext _context;

        public StatisticWindow(User user, ApContext context)
        {
            InitializeComponent();
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            LoadInitialData();
            StatTypeSelector.SelectedIndex = 0; // Mặc định chọn Total Salary by Department
            LoadStatisticsChart();
        }

        private void LoadInitialData()
        {
            var employees = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Job)
                .Where(e => e.ManagerId == _currentUser.EmployeeId)
                .ToList();

            decimal totalSalary = employees.Sum(e => e.Salary);
            int totalEmployees = employees.Count;
            int totalCategories = employees.Select(e => e.Department?.DepartmentName).Distinct().Count() +
                                 employees.Select(e => e.Job?.JobTitle).Distinct().Count();
            decimal avgSalary = totalEmployees > 0 ? totalSalary / totalEmployees : 0;

            TotalSalaryText.Text = totalSalary.ToString("C0", CultureInfo.GetCultureInfo("en-US"));
            TotalEmployeesText.Text = totalEmployees.ToString();
            TotalCategoriesText.Text = totalCategories.ToString();
            AvgSalaryText.Text = avgSalary.ToString("C0", CultureInfo.GetCultureInfo("en-US"));
        }

        private void StatTypeSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadStatisticsChart();
        }

        private void LoadStatisticsChart()
        {
            var statType = StatTypeSelector.SelectedItem as ComboBoxItem;
            if (statType == null) return;

            string selectedStat = statType.Content.ToString();
            StatisticsChart.Series.Clear();
            StatisticsChart.AxisX.Clear();
            StatisticsChart.AxisY.Clear();
            PieChart.Series.Clear();

            switch (selectedStat)
            {
                case "💰 Total Salary by Department":
                    var salaryStats = _context.Employees
                        .Include(e => e.Department)
                        .Where(e => e.ManagerId == _currentUser.EmployeeId)
                        .GroupBy(e => e.Department != null ? e.Department.DepartmentName : "Unknown")
                        .Select(g => new { Department = g.Key, TotalSalary = g.Sum(e => (decimal?)e.Salary) ?? 0 })
                        .ToList();

                    var deptLabels = salaryStats.Select(s => s.Department).ToArray();
                    var deptData = new ChartValues<decimal>(salaryStats.Select(s => Math.Round(s.TotalSalary, 2)));

                    StatisticsChart.Series.Add(new ColumnSeries
                    {
                        Title = "Total Salary",
                        Values = deptData,
                        Fill = System.Windows.Media.Brushes.SkyBlue
                    });

                    StatisticsChart.AxisX.Add(new Axis
                    {
                        Title = "Department",
                        Labels = deptLabels
                    });

                    StatisticsChart.AxisY.Add(new Axis
                    {
                        Title = "Salary",
                        LabelFormatter = value => value.ToString("C0", CultureInfo.GetCultureInfo("en-US")),
                        MinValue = 0
                    });
                    StatisticsChart.LegendLocation = LegendLocation.Top;
                    break;

                case "👔 Employee by Job Title":
                    var jobStats = _context.Employees
                        .Include(e => e.Job)
                        .Where(e => e.ManagerId == _currentUser.EmployeeId)
                        .GroupBy(e => e.Job != null ? e.Job.JobTitle : "Unknown")
                        .Select(g => new { JobTitle = g.Key, Count = g.Count() })
                        .ToList();

                    var jobLabels = jobStats.Select(j => j.JobTitle ?? "Unknown").ToArray();
                    var jobData = new ChartValues<int>(jobStats.Select(j => j.Count));

                    StatisticsChart.Series.Add(new ColumnSeries
                    {
                        Title = "Employee Count",
                        Values = jobData,
                        Fill = System.Windows.Media.Brushes.Gold
                    });

                    StatisticsChart.AxisX.Add(new Axis
                    {
                        Title = "Job Title",
                        Labels = jobLabels
                    });

                    StatisticsChart.AxisY.Add(new Axis
                    {
                        Title = "Count",
                        LabelFormatter = value => value.ToString("N0"),
                        MinValue = 0
                    });
                    StatisticsChart.LegendLocation = LegendLocation.Top;
                    break;

                case "🔄 Combined Overview":
                    var combinedStats = _context.Employees
                        .Include(e => e.Department)
                        .Where(e => e.ManagerId == _currentUser.EmployeeId)
                        .GroupBy(e => e.Department != null ? e.Department.DepartmentName : "Unknown")
                        .Select(g => new { Department = g.Key, TotalSalary = g.Sum(e => (decimal?)e.Salary) ?? 0 })
                        .ToList();

                    var totalSalary = combinedStats.Sum(s => s.TotalSalary);
                    var pieValues = new ChartValues<decimal>(combinedStats.Select(s => Math.Round(s.TotalSalary, 2)));
                    var pieLabels = combinedStats.Select(s => s.Department).ToArray();

                    PieChart.Series = new SeriesCollection
                    {
                        new PieSeries
                        {
                            Title = "Salary Distribution",
                            Values = pieValues,
                            DataLabels = true,
                            LabelPoint = chartPoint => $"{chartPoint.Y.ToString("C0", CultureInfo.GetCultureInfo("en-US"))} ({chartPoint.Participation:P2})",
                            Fill = System.Windows.Media.Brushes.SkyBlue
                        }
                    };

                    PieChart.LegendLocation = LegendLocation.Top;
                    break;
            }
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            LoadInitialData();
            LoadStatisticsChart();
            MessageBox.Show("Data has been refreshed!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportChart_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export feature is under development.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}