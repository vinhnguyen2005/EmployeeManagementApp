using System;
using System.Collections.Generic;

namespace ConsoleApp.Models;

public partial class AttendanceLog
{
    public int LogId { get; set; }

    public int EmployeeId { get; set; }

    public DateTime CheckInTime { get; set; }

    public double SimilarityScore { get; set; }

    public string? Status { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
