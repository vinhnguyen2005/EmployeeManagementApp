using Xunit;
using Moq;
using FluentAssertions;
using ConsoleApp;
using ConsoleApp.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace XUnitTest
{
    public class JobServiceTests
    {
        private readonly Mock<EmployeeContext> _mockContext;
        private readonly Mock<DbSet<Job>> _mockJobSet;
        private readonly Mock<DbSet<Employee>> _mockEmployeeSet;
        private readonly JobService _service;

        public JobServiceTests()
        {
            _mockContext = new Mock<EmployeeContext>();
            _mockJobSet = new Mock<DbSet<Job>>();
            _mockEmployeeSet = new Mock<DbSet<Employee>>();
            _service = new JobService(_mockContext.Object);
        }

        private void SetupMockDbSet<T>(Mock<DbSet<T>> mockSet, List<T> sourceList, Mock<EmployeeContext> mockContext, Expression<Func<EmployeeContext, DbSet<T>>> dbSetSelector) where T : class
        {
            var queryable = sourceList.AsQueryable();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            mockSet.Setup(s => s.Find(It.IsAny<object[]>()))
                   .Returns<object[]>(key => sourceList.FirstOrDefault(e => {
                       var property = typeof(T).GetProperty("JobId");
                       if (property != null)
                       {
                           return (int)property.GetValue(e) == (int)key[0];
                       }
                       property = typeof(T).GetProperty("EmployeeId");
                       if (property != null)
                       {
                           return (int)property.GetValue(e) == (int)key[0];
                       }
                       return false;
                   }));

            mockSet.Setup(s => s.Add(It.IsAny<T>())).Callback<T>(sourceList.Add);
            mockSet.Setup(s => s.Remove(It.IsAny<T>())).Callback<T>(item => sourceList.Remove(item));
            mockContext.Setup(dbSetSelector).Returns(mockSet.Object);
        }

        //================================================================================
        // Test cho phương thức GetAllJobs 
        //================================================================================

        [Fact]
        public void GetAllJobs_WhenJobsExist_ReturnsSortedListOfJobs()
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Zzz Last Job", MinSalary = 50000, MaxSalary = 80000 },
                new Job { JobId = 2, JobTitle = "Aaa First Job", MinSalary = 60000, MaxSalary = 90000 },
                new Job { JobId = 3, JobTitle = "Mmm Middle Job", MinSalary = 55000, MaxSalary = 85000 },
                new Job { JobId = 4, JobTitle = "Developer", MinSalary = 70000, MaxSalary = 120000 }
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            var result = _service.GetAllJobs();
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result.Should().BeInAscendingOrder(j => j.JobTitle, "danh sách công việc phải được sắp xếp theo JobTitle.");
            result.First().JobTitle.Should().Be("Aaa First Job");
            result.Last().JobTitle.Should().Be("Zzz Last Job");
        }

        [Fact]
        public void GetAllJobs_WhenNoJobsExist_ReturnsEmptyList()
        {
            var jobs = new List<Job>();
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            var result = _service.GetAllJobs();
            result.Should().NotBeNull();
            result.Should().BeEmpty("phải trả về danh sách rỗng khi không có công việc nào.");
        }

        [Fact]
        public void GetAllJobs_WhenSingleJobExists_ReturnsSingleJobList()
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Only Job", MinSalary = 50000, MaxSalary = 80000 }
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            var result = _service.GetAllJobs();
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().JobTitle.Should().Be("Only Job");
        }

        [Fact]
        public void GetAllJobs_WhenJobsHaveSameTitles_HandlesDuplicateTitles()
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Developer", MinSalary = 50000, MaxSalary = 80000 },
                new Job { JobId = 2, JobTitle = "Developer", MinSalary = 60000, MaxSalary = 90000 },
                new Job { JobId = 3, JobTitle = "Tester", MinSalary = 45000, MaxSalary = 75000 }
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            var result = _service.GetAllJobs();
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeInAscendingOrder(j => j.JobTitle);
        }

        //================================================================================
        // Test cho phương thức AddJob 
        //================================================================================

        [Fact]
        public void AddJob_WithValidData_AddsJobAndReturnsIt()
        {

            var jobs = new List<Job>();
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            var result = _service.AddJob("Software Engineer", 50000, 100000);
            result.Should().NotBeNull();
            result.JobTitle.Should().Be("Software Engineer");
            result.MinSalary.Should().Be(50000);
            result.MaxSalary.Should().Be(100000);
            jobs.Should().ContainSingle(j => j.JobTitle == "Software Engineer");
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("  \t  \n  ")]
        public void AddJob_WithInvalidTitle_ThrowsArgumentException(string invalidTitle)
        {
            Action act = () => _service.AddJob(invalidTitle, 50000, 100000);
            act.Should().Throw<ArgumentException>().WithMessage("Job title cannot be empty.");
        }

        [Theory]
        [InlineData(100000, 50000)] 
        [InlineData(75000, 74999)]  
        [InlineData(0, -1)]         
        public void AddJob_WithMinSalaryGreaterThanMaxSalary_ThrowsArgumentException(decimal minSalary, decimal maxSalary)
        {
            Action act = () => _service.AddJob("Manager", minSalary, maxSalary);
            act.Should().Throw<ArgumentException>().WithMessage("Minimum salary cannot be greater than maximum salary.");
        }

        [Theory]
        [InlineData(50000, 50000)]   
        [InlineData(0, 0)]           
        [InlineData(1, 1)]           
        public void AddJob_WithMinSalaryEqualToMaxSalary_AddsJobSuccessfully(decimal minSalary, decimal maxSalary)
        {
            var jobs = new List<Job>();
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            var result = _service.AddJob("Equal Salary Job", minSalary, maxSalary);
            result.Should().NotBeNull();
            result.MinSalary.Should().Be(minSalary);
            result.MaxSalary.Should().Be(maxSalary);
            jobs.Should().ContainSingle();
        }


        [Fact]
        public void AddJob_WithVeryLongJobTitle_AddsJobSuccessfully()
        {
            var jobs = new List<Job>();
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            var longTitle = new string('A', 500); 
            var result = _service.AddJob(longTitle, 50000, 100000);
            result.Should().NotBeNull();
            result.JobTitle.Should().Be(longTitle);
            jobs.Should().ContainSingle();
        }

        [Fact]
        public void AddJob_WithSpecialCharactersInTitle_AddsJobSuccessfully()
        {
            var jobs = new List<Job>();
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            var specialTitle = "C# .NET Developer (Senior) - Team Lead & Architect 100%";
            var result = _service.AddJob(specialTitle, 50000, 100000);
            result.Should().NotBeNull();
            result.JobTitle.Should().Be(specialTitle);
            jobs.Should().ContainSingle();
        }

        [Fact]
        public void AddJob_WhenDatabaseThrowsException_PropagatesException()
        {
            var jobs = new List<Job>();
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            _mockContext.Setup(c => c.SaveChanges()).Throws(new InvalidOperationException("Database error"));
            Action act = () => _service.AddJob("Test Job", 50000, 100000);
            act.Should().Throw<InvalidOperationException>().WithMessage("Database error");
        }

        //================================================================================
        // Test cho phương thức UpdateJob
        //================================================================================

        [Fact]
        public void UpdateJob_WhenJobExists_UpdatesAndReturnsJob()
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Old Title", MinSalary = 1000, MaxSalary = 2000 },
                new Job { JobId = 2, JobTitle = "Another Job", MinSalary = 3000, MaxSalary = 4000 }
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            var result = _service.UpdateJob(1, "New Title", 1500, 2500);
            result.Should().NotBeNull();
            result.JobId.Should().Be(1);
            result.JobTitle.Should().Be("New Title");
            result.MinSalary.Should().Be(1500);
            result.MaxSalary.Should().Be(2500);
            var unchangedJob = jobs.First(j => j.JobId == 2);
            unchangedJob.JobTitle.Should().Be("Another Job");
            unchangedJob.MinSalary.Should().Be(3000);

            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateJob_WhenJobNotFound_ThrowsKeyNotFoundException()
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Existing Job", MinSalary = 1000, MaxSalary = 2000 }
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            Action act = () => _service.UpdateJob(99, "Non-existent job", 50000, 100000);
            act.Should().Throw<KeyNotFoundException>();
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public void UpdateJob_WithInvalidTitle_ThrowsArgumentException(string invalidTitle)
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Valid Job", MinSalary = 1000, MaxSalary = 2000 }
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            Action act = () => _service.UpdateJob(1, invalidTitle, 50000, 100000);
            act.Should().Throw<ArgumentException>().WithMessage("Job title cannot be empty.");
        }

        [Theory]
        [InlineData(100000, 50000)]
        [InlineData(75000, 74999)]
        [InlineData(1, 0)]
        public void UpdateJob_WithInvalidSalaryRange_ThrowsArgumentException(decimal minSalary, decimal maxSalary)
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Valid Job", MinSalary = 1000, MaxSalary = 2000 }
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            Action act = () => _service.UpdateJob(1, "Updated Job", minSalary, maxSalary);
            act.Should().Throw<ArgumentException>().WithMessage("Minimum salary cannot be greater than maximum salary.");
        }

        [Fact]
        public void UpdateJob_WithSameValues_UpdatesSuccessfully()
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Same Job", MinSalary = 1000, MaxSalary = 2000 }
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            var result = _service.UpdateJob(1, "Same Job", 1000, 2000);
            result.Should().NotBeNull();
            result.JobTitle.Should().Be("Same Job");
            result.MinSalary.Should().Be(1000);
            result.MaxSalary.Should().Be(2000);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateJob_WhenDatabaseThrowsException_PropagatesException()
        {
            // Arrange
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Test Job", MinSalary = 1000, MaxSalary = 2000 }
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            _mockContext.Setup(c => c.SaveChanges()).Throws(new InvalidOperationException("Database connection lost"));

            // Act & Assert
            Action act = () => _service.UpdateJob(1, "Updated Job", 1500, 2500);
            act.Should().Throw<InvalidOperationException>().WithMessage("Database connection lost");
        }

        //================================================================================
        // Test cho phương thức DeleteJob
        //================================================================================

        [Fact]
        public void DeleteJob_WhenJobExistsAndNotInUse_DeletesJob()
        {
            // Arrange
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Obsolete Job" },
                new Job { JobId = 2, JobTitle = "Active Job" }
            };
            var employees = new List<Employee>
            {
                new Employee { EmployeeId = 101, JobId = 2 } 
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            SetupMockDbSet(_mockEmployeeSet, employees, _mockContext, c => c.Employees);

            // Act
            _service.DeleteJob(1);

            // Assert
            jobs.Should().HaveCount(1);
            jobs.Should().NotContain(j => j.JobId == 1);
            jobs.Should().Contain(j => j.JobId == 2); 
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteJob_WhenJobIsInUse_ThrowsInvalidOperationException()
        {
            // Arrange
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Developer" }
            };
            var employees = new List<Employee>
            {
                new Employee { EmployeeId = 101, JobId = 1 },
                new Employee { EmployeeId = 102, JobId = 1 } 
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            SetupMockDbSet(_mockEmployeeSet, employees, _mockContext, c => c.Employees);
            Action act = () => _service.DeleteJob(1);
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Cannot delete this job because it is currently assigned to one or more employees.");
            jobs.Should().HaveCount(1);
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [Fact]
        public void DeleteJob_WhenJobNotFound_ThrowsKeyNotFoundException()
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Existing Job" }
            };
            var employees = new List<Employee>();
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            SetupMockDbSet(_mockEmployeeSet, employees, _mockContext, c => c.Employees);
            Action act = () => _service.DeleteJob(99);
            act.Should().Throw<KeyNotFoundException>();
            jobs.Should().HaveCount(1);
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [Fact]
        public void DeleteJob_WhenMultipleJobsExistButOnlyOneInUse_DeletesCorrectJob()
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Safe to Delete" },
                new Job { JobId = 2, JobTitle = "In Use Job" },
                new Job { JobId = 3, JobTitle = "Also Safe" }
            };
            var employees = new List<Employee>
            {
                new Employee { EmployeeId = 101, JobId = 2 } 
            };
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            SetupMockDbSet(_mockEmployeeSet, employees, _mockContext, c => c.Employees);
            _service.DeleteJob(1);
            jobs.Should().HaveCount(2);
            jobs.Should().NotContain(j => j.JobId == 1);
            jobs.Should().Contain(j => j.JobId == 2);
            jobs.Should().Contain(j => j.JobId == 3);
        }

        [Fact]
        public void DeleteJob_WhenDatabaseThrowsException_PropagatesException()
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Test Job" }
            };
            var employees = new List<Employee>();
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            SetupMockDbSet(_mockEmployeeSet, employees, _mockContext, c => c.Employees);
            _mockContext.Setup(c => c.SaveChanges()).Throws(new InvalidOperationException("Database transaction failed"));
            Action act = () => _service.DeleteJob(1);
            act.Should().Throw<InvalidOperationException>().WithMessage("Database transaction failed");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void DeleteJob_WithInvalidJobId_ThrowsKeyNotFoundException(int invalidJobId)
        {
            var jobs = new List<Job>
            {
                new Job { JobId = 1, JobTitle = "Valid Job" }
            };
            var employees = new List<Employee>();
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            SetupMockDbSet(_mockEmployeeSet, employees, _mockContext, c => c.Employees);
            Action act = () => _service.DeleteJob(invalidJobId);
            act.Should().Throw<KeyNotFoundException>();
        }

        //================================================================================
        // Integration Tests - Testing multiple operations together
        //================================================================================
        [Fact]
        public void JobService_CompleteWorkflow_WorksCorrectly()
        {
            var jobs = new List<Job>();
            var employees = new List<Employee>();
            SetupMockDbSet(_mockJobSet, jobs, _mockContext, c => c.Jobs);
            SetupMockDbSet(_mockEmployeeSet, employees, _mockContext, c => c.Employees);
            var job1 = _service.AddJob("Developer", 50000, 80000);
            var job2 = _service.AddJob("Tester", 40000, 70000);
            var job3 = _service.AddJob("Manager", 60000, 100000);
            jobs.Should().HaveCount(3);
            var allJobs = _service.GetAllJobs();
            allJobs.Should().HaveCount(3);
            allJobs.Should().BeInAscendingOrder(j => j.JobTitle);
            var updatedJob = _service.UpdateJob(job1.JobId, "Senior Developer", 60000, 90000);
            updatedJob.JobTitle.Should().Be("Senior Developer");
            Action deleteNonExistent = () => _service.DeleteJob(999);
            deleteNonExistent.Should().Throw<KeyNotFoundException>();
            var initialJobCount = jobs.Count;
            jobs.Should().HaveCount(3);
        }
    }
}