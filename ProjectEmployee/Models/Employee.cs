using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectEmployee.Models
{
    public partial class Employee
    {
        public Employee()
        {
            Dependents = new HashSet<Dependent>();
            InverseManager = new HashSet<Employee>();
            Tasks = new HashSet<TaskAssign>();
            Users = new HashSet<User>();
            RequestsSubject = new HashSet<Request>();
            RequestsToApprove = new HashSet<Request>();
            RequestsCreated = new HashSet<Request>();
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
        public bool IsActive { get; set; }
        public byte[]? FaceEncoding { get; set; }

        public string? ProfilePicturePath { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public virtual Department? Department { get; set; }
        public virtual Job Job { get; set; } = null!;
        public virtual Employee? Manager { get; set; }
        public virtual ICollection<Dependent> Dependents { get; set; }
        public virtual ICollection<Employee> InverseManager { get; set; }
        public virtual ICollection<TaskAssign> Tasks { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Request> RequestsSubject { get; set; }
        public virtual ICollection<Request> RequestsToApprove { get; set; }
        public virtual ICollection<Request> RequestsCreated { get; set; }
    }
}
