﻿using System;
using System.Collections.Generic;

namespace ProjectEmployee.Models
{
    public partial class User
    {
        public User()
        {
            UserRoles = new HashSet<UserRole>();
        }

        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public int? EmployeeId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public virtual Employee? Employee { get; set; }
        public bool IsLocked { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
