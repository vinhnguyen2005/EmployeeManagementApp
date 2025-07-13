using System;
using System.Globalization;
using System.Windows.Data;
using ProjectEmployee.Models;
namespace ProjectEmployee.Formatter
{
    public class TaskToStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskAssign task)
            {
                if (task.Status.ToLower() == "completed")
                {
                    if (task.CompletedDate.HasValue)
                    {
                        if (task.Deadline.Date < task.CompletedDate.Value.Date)
                        {
                            return $"Completed (Late) on {task.CompletedDate:dd/MM/yyyy}";
                        }
                        return $"Completed on {task.CompletedDate:dd/MM/yyyy}";
                    }
                    return "Completed";
                }
                return task.Status;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}