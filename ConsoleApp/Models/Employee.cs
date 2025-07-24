using System;
using System.Collections.Generic;

namespace ConsoleApp.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string? FirstName { get; set; }

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public DateOnly HireDate { get; set; }

    public int JobId { get; set; }

    public decimal Salary { get; set; }

    public int? ManagerId { get; set; }

    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; }

    public string? ProfilePicturePath { get; set; }

    public byte[]? FaceEncoding { get; set; }

    public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Dependent> Dependents { get; set; } = new List<Dependent>();

    public virtual ICollection<Employee> InverseManager { get; set; } = new List<Employee>();

    public virtual Job Job { get; set; } = null!;

    public virtual Employee? Manager { get; set; }

    public virtual ICollection<Request> RequestEmployees { get; set; } = new List<Request>();

    public virtual ICollection<Request> RequestManagers { get; set; } = new List<Request>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
