using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace ProjectEmployee.Models
{
    public partial class ApContext : DbContext
    {
        public ApContext() { }
        public ApContext(DbContextOptions<ApContext> options) : base(options) { }

        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Dependent> Dependents { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<TaskAssign> Tasks { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }

        public virtual DbSet<AuditLog> AuditLogs { get; set; }
        public virtual DbSet<AttendanceLog> AttendanceLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                if (!optionsBuilder.IsConfigured)
                {
                    var connectionString = config.GetConnectionString("DB");
                    optionsBuilder.UseSqlServer(connectionString);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi đọc appsettings.json: " + ex.Message);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.CountryId).HasName("PK__countrie__7E8CD0555E6330D6");
                entity.ToTable("countries");
                entity.Property(e => e.CountryId)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("country_id");
                entity.Property(e => e.CountryName)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("country_name");
                entity.Property(e => e.RegionId).HasColumnName("region_id");
                entity.HasOne(d => d.Region).WithMany(p => p.Countries)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("FK__countries__regio__3B75D760");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepartmentId).HasName("PK__departme__C223242294CABE85");
                entity.ToTable("departments");
                entity.Property(e => e.DepartmentId).HasColumnName("department_id");
                entity.Property(e => e.DepartmentName)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("department_name");
                entity.Property(e => e.LocationId)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("location_id");
                entity.HasOne(d => d.Location).WithMany(p => p.Departments)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__departmen__locat__48CFD27E");
            });

            modelBuilder.Entity<Dependent>(entity =>
            {
                entity.HasKey(e => e.DependentId).HasName("PK__dependen__F25E28CEB683689E");
                entity.ToTable("dependents");
                entity.Property(e => e.DependentId).HasColumnName("dependent_id");
                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("first_name");
                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("last_name");
                entity.Property(e => e.Relationship)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("relationship");
                entity.HasOne(d => d.Employee).WithMany(p => p.Dependents)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__dependent__emplo__5441852A");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId).HasName("PK__employee__C52E0BA8E4B27159");
                entity.ToTable("employees");
                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
                entity.Property(e => e.DepartmentId)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("department_id");
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("email");
                entity.Property(e => e.FirstName)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("first_name");
                entity.Property(e => e.HireDate).HasColumnName("hire_date");
                entity.Property(e => e.JobId).HasColumnName("job_id");
                entity.Property(e => e.LastName)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("last_name");
                entity.Property(e => e.ManagerId)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("manager_id");
                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("phone_number");
                entity.Property(e => e.Salary)
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("salary");

                entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__employees__depar__5070F446");
                entity.HasOne(d => d.Job).WithMany(p => p.Employees)
                    .HasForeignKey(d => d.JobId)
                    .HasConstraintName("FK__employees__job_i__4F7CD00D");
                entity.HasOne(d => d.Manager).WithMany(p => p.InverseManager)
                    .HasForeignKey(d => d.ManagerId)
                    .HasConstraintName("FK__employees__manag__5165187F");
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.JobId).HasName("PK__jobs__6E32B6A5CFCCABBF");
                entity.ToTable("jobs");
                entity.Property(e => e.JobId).HasColumnName("job_id");
                entity.Property(e => e.JobTitle)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .HasColumnName("job_title");
                entity.Property(e => e.MaxSalary)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("max_salary");
                entity.Property(e => e.MinSalary)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("min_salary");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.LocationId).HasName("PK__location__771831EA7FE483F4");
                entity.ToTable("locations");
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.City)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("city");
                entity.Property(e => e.CountryId)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("country_id");
                entity.Property(e => e.PostalCode)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("postal_code");
                entity.Property(e => e.StateProvince)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("state_province");
                entity.Property(e => e.StreetAddress)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("street_address");
                entity.HasOne(d => d.Country).WithMany(p => p.Locations)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK__locations__count__412EB0B6");
            });

            modelBuilder.Entity<Region>(entity =>
            {
                entity.HasKey(e => e.RegionId).HasName("PK__regions__01146BAEFD21050C");
                entity.ToTable("regions");
                entity.Property(e => e.RegionId).HasColumnName("region_id");
                entity.Property(e => e.RegionName)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("region_name");
            });
            modelBuilder.Entity<Request>(entity =>
            {
                entity.HasKey(e => e.RequestId);
                entity.ToTable("requests");
                entity.Property(e => e.RequestId).HasColumnName("request_id");
                entity.Property(e => e.OriginatorId).HasColumnName("OriginatorId");
                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
                entity.Property(e => e.ManagerId).HasColumnName("manager_id");
                entity.Property(e => e.RequestType).HasColumnName("request_type");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.RaiseAmount).HasColumnName("raise_amount");
                entity.Property(e => e.StartDate).HasColumnName("StartDate");
                entity.Property(e => e.EndDate).HasColumnName("EndDate");
                entity.HasOne(r => r.Employee)
                      .WithMany(e => e.RequestsSubject) 
                      .HasForeignKey(r => r.EmployeeId)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_Requests_Employee");

                entity.HasOne(r => r.Manager)
                      .WithMany(e => e.RequestsToApprove)
                      .HasForeignKey(r => r.ManagerId)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_Requests_Manager");

                entity.HasOne(r => r.Originator)
                      .WithMany(e => e.RequestsCreated) 
                      .HasForeignKey(r => r.OriginatorId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .HasConstraintName("FK_Requests_Originator");
            });

            modelBuilder.Entity<TaskAssign>(entity =>
            {
                entity.HasKey(e => e.TaskId).HasName("PK__tasks__task_id");

                entity.ToTable("tasks");

                entity.Property(e => e.TaskId).HasColumnName("task_id");
                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
                entity.Property(e => e.TaskDescription)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("task_description");
                entity.Property(e => e.Deadline).HasColumnName("deadline");
                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("Pending")
                    .HasColumnName("status");
                entity.Property(e => e.Priority)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("Medium")
                    .HasColumnName("priority");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("created_date");
                entity.Property(e => e.CompletedDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("completed_date");
                entity.Property(e => e.PerformanceScore)
                    .HasColumnType("decimal(5, 2)")
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnName("performance_score");
                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__tasks__employee_id");
                entity.Property(e => e.ReviewComment)
                    .HasColumnName("review_comment")
                    .HasMaxLength(500);

            });
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.LogId);

                entity.ToTable("AuditLogs");
                entity.Property(e => e.LogId).HasColumnName("LogId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.Username).HasColumnName("Username");
                entity.Property(e => e.ActionType).HasColumnName("ActionType");
                entity.Property(e => e.Details).HasColumnName("Details");
                entity.Property(e => e.Timestamp).HasColumnName("Timestamp");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370FA07B1DE2");
                entity.ToTable("users");
                entity.HasIndex(e => e.Username, "UQ__users__F3DBC572DE0F55F4").IsUnique();
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("created_at");
                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsUnicode(false).HasColumnName("password_hash");
                entity.Property(e => e.Username).HasMaxLength(50).IsUnicode(false).HasColumnName("username");
                entity.HasOne(d => d.Employee).WithMany(p => p.Users)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__users__employee___6B24EA82");
            });
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.RoleName)
                    .HasColumnName("role_name")
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles");
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.Property(ur => ur.UserId).HasColumnName("user_id");
                entity.Property(ur => ur.RoleId).HasColumnName("role_id");

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
