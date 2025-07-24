using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConsoleApp.Models;

public partial class EmployeeContext : DbContext
{
    public EmployeeContext()
    {
    }

    public EmployeeContext(DbContextOptions<EmployeeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttendanceLog> AttendanceLogs { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Dependent> Dependents { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(config.GetConnectionString("DB"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttendanceLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.HasIndex(e => e.EmployeeId, "IX_AttendanceLogs_EmployeeId");

            entity.HasOne(d => d.Employee).WithMany(p => p.AttendanceLogs)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_AttendanceLogs_Employees_EmployeeId");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E548648691A219E");

            entity.Property(e => e.ActionType).HasMaxLength(100);
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

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
            entity.Property(e => e.IsActive).HasDefaultValue(true);
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
            entity.HasKey(e => e.RequestId).HasName("PK__requests__18D3B90FD7E115FA");

            entity.ToTable("requests");

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.RaiseAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("raise_amount");
            entity.Property(e => e.RequestType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("request_type");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Employee).WithMany(p => p.RequestEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__requests__employ__6FE99F9F");

            entity.HasOne(d => d.Manager).WithMany(p => p.RequestManagers)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__requests__manage__70DDC3D8");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__roles__760965CC8DAC9B21");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "UQ__roles__783254B166FF5C8D").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__tasks__0492148DFB98DEC1");

            entity.ToTable("tasks");

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.CompletedDate)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("completed_date");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_date");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.PerformanceScore)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("performance_score");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Medium")
                .HasColumnName("priority");
            entity.Property(e => e.ReviewComment)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("review_comment");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.TaskDescription)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("task_description");

            entity.HasOne(d => d.Employee).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__tasks__employee___07C12930");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370F822B1430");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "UQ__users__F3DBC5722796BE70").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Employee).WithMany(p => p.Users)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__users__employee___31B762FC");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__user_role__role___3864608B"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__user_role__user___37703C52"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__user_rol__6EDEA15347C79EF7");
                        j.ToTable("user_roles");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
