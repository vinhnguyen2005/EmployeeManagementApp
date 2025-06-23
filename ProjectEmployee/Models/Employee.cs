using System;
using System.Collections.Generic;

namespace ProjectEmployee.Models;

public partial class Employee
{
    private ICollection<TaskAssign> tasks;

    public Employee()
    {
        Dependents = new List<Dependent>();
        InverseManager = new List<Employee>();
        RequestsSent = new List<Request>();
        RequestsReceived = new List<Request>();
        Tasks = new List<TaskAssign>();
        Users = new List<User>();
    }

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

    public virtual Department? Department { get; set; }

    public virtual ICollection<Dependent> Dependents { get; set; }

    public virtual ICollection<Employee> InverseManager { get; set; }

    public virtual ICollection<Request> RequestsSent { get; set; }
    public virtual ICollection<Request> RequestsReceived { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual Employee? Manager { get; set; }

    public virtual ICollection<User> Users { get; set; }

    public ICollection<TaskAssign> Tasks
    {
        get => tasks;
        set => tasks = value;
    }
}
