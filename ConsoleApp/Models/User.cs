using System;
using System.Collections.Generic;

namespace ConsoleApp.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int? EmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsLocked { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
