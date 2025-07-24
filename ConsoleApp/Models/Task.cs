using System;
using System.Collections.Generic;

namespace ConsoleApp.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public int EmployeeId { get; set; }

    public string TaskDescription { get; set; } = null!;

    public DateOnly Deadline { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public DateOnly? CompletedDate { get; set; }

    public decimal? PerformanceScore { get; set; }

    public string? ReviewComment { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
