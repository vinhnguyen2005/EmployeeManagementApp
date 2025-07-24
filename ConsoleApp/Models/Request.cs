using System;
using System.Collections.Generic;

namespace ConsoleApp.Models;

public partial class Request
{
    public int RequestId { get; set; }

    public int EmployeeId { get; set; }

    public int ManagerId { get; set; }

    public string RequestType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public decimal? RaiseAmount { get; set; }

    public int? OriginatorId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Employee Manager { get; set; } = null!;
}
