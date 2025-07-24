using ProjectEmployee.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectEmployee.HR
{

    public class AbsenteeViewModel
    {
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string DepartmentName { get; set; }
        public string Status { get; set; }
    }

    public partial class AttendanceLogWindow : Window
    {
        private readonly ApContext _context;

        public AttendanceLogWindow()
        {
            InitializeComponent();
            _context = new ApContext();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateEmployeeFilter();
            LoadLogs();
        }

        private void PopulateEmployeeFilter()
        {
            var employees = _context.Employees.Where(e => e.IsActive).OrderBy(e => e.FirstName).ToList();
            employees.Insert(0, new Employee { EmployeeId = 0, FirstName = "All", LastName = "Employees" });
            cboEmployeeFilter.ItemsSource = employees;
            cboEmployeeFilter.DisplayMemberPath = "FullName";
            cboEmployeeFilter.SelectedValuePath = "EmployeeId";
            cboEmployeeFilter.SelectedIndex = 0;
        }

        private void LoadLogs()
        {
            if (!IsLoaded) return;

            try
            {
                var query = _context.AttendanceLogs.Include(log => log.Employee).AsQueryable();

                if (dpDateFilter.SelectedDate.HasValue)
                {
                    DateTime selectedDate = dpDateFilter.SelectedDate.Value.Date;
                    query = query.Where(log => log.CheckInTime.Date == selectedDate);
                }

                if (cboEmployeeFilter.SelectedIndex > 0)
                {
                    int selectedEmployeeId = (int)cboEmployeeFilter.SelectedValue;
                    query = query.Where(log => log.EmployeeId == selectedEmployeeId);
                }

                dgAttendanceLogs.ItemsSource = query.OrderByDescending(log => log.CheckInTime).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }


        private void FindAbsentees_Click(object sender, RoutedEventArgs e)
        {
            if (!dpDateFilter.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a specific date to find absentees.", "Date Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime selectedDate = dpDateFilter.SelectedDate.Value.Date;

            try
            {
                var allActiveEmployees = _context.Employees
                    .Include(e => e.Job)
                    .Include(e => e.Department)
                    .Where(e => e.IsActive)
                    .ToList();
                var employeesWhoCheckedInIds = _context.AttendanceLogs
                    .Where(log => log.CheckInTime.Date == selectedDate)
                    .Select(log => log.EmployeeId)
                    .Distinct()
                    .ToList();
                var employeesOnLeaveIds = _context.Requests
                    .Where(req => req.Status == "Approved" &&
                                  (req.RequestType == "Leave" || req.RequestType == "Work From Home") &&
                                  req.StartDate.HasValue && req.EndDate.HasValue &&
                                  selectedDate >= req.StartDate.Value.Date &&
                                  selectedDate <= req.EndDate.Value.Date)
                    .Select(req => req.EmployeeId)
                    .Distinct()
                    .ToList();

                var absentees = allActiveEmployees
                    .Where(emp => !employeesWhoCheckedInIds.Contains(emp.EmployeeId) &&
                                  !employeesOnLeaveIds.Contains(emp.EmployeeId))
                    .ToList();

                var absenteeReport = absentees.Select(emp => new AbsenteeViewModel
                {
                    FullName = emp.FullName,
                    JobTitle = emp.Job?.JobTitle ?? "N/A",
                    DepartmentName = emp.Department?.DepartmentName ?? "N/A",
                    Status = "Absent without leave"
                }).ToList();

                if (absenteeReport.Any())
                {
                    string report = string.Join("\n", absenteeReport.Select(a => a.FullName));
                    MessageBox.Show($"Found {absenteeReport.Count} employee(s) absent without leave on {selectedDate:dd/MM/yyyy}:\n\n{report}", "Absentee Report");
                }
                else
                {
                    MessageBox.Show($"No employees were found absent without leave on {selectedDate:dd/MM/yyyy}.", "Absentee Report");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while finding absentees: {ex.Message}");
            }
        }

        private void Filters_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadLogs();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            dpDateFilter.SelectedDate = null;
            cboEmployeeFilter.SelectedIndex = 0;
            LoadLogs();
        }
    }
}
