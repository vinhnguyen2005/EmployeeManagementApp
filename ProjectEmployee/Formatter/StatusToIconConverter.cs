using System;
using System.Globalization;
using System.Windows.Data;
namespace ProjectEmployee.Formatter
{ 
    public class StatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "completed" => "✅",
                    "in progress" => "🔄",
                    "pending" => "⏳",
                    _ => "📋"
                };
            }
            return "📋";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}