using ProjectEmployee.Models;
using System.Linq;
using System.Windows;

namespace ProjectEmployee.Admin
{
    public partial class SystemLogsWindow : Window
    {
        private readonly ApContext _context;

        public SystemLogsWindow()
        {
            InitializeComponent();
            _context = new ApContext();
            LoadLogs();
        }

        private void LoadLogs()
        {
            dgLogs.ItemsSource = _context.AuditLogs
                .OrderByDescending(log => log.Timestamp)
                .Take(1000) 
                .ToList();
        }
    }
}
