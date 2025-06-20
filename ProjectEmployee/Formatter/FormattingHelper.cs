using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectEmployee.Formatter
{
    public static class FormattingHelper
    {
        public static string FormatAsInteger(decimal? value)
        {
            return value.HasValue ? Math.Floor(value.Value).ToString("N0") : string.Empty;
        }

        public static string FormatAsCurrency(decimal? value)
        {
            return value.HasValue ? value.Value.ToString("C") : string.Empty;
        }

        public static string FormatAsDollarInteger(decimal? value)
        {
            return value.HasValue ? $"${Math.Floor(value.Value).ToString("N0")}" : string.Empty;
        }
    }
}
