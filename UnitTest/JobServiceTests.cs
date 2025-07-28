using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace UnitTest
{
    public class JobServiceTests : IDisposable
    {
        private readonly EmployeeContext _context;
        private readonly JobService _jobService;

        public JobServiceTests()
        {
            // Tạo in-memory database cho testing
            var options = new DbContextOptionsBuilder<EmployeeContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EmployeeContext(options);
            _jobService = new JobService(_context);
        }

        [Fact]
        public void GetAllJobs_WhenJobsExist_ReturnsJobsOrderedByTitle()
        {
            // Arrange
            _context.Jobs.AddRange(new[]
            {
                new Job { JobTitle = "Manager", MinSalary = 50000, MaxSalary = 80000 },
                new Job { JobTitle = "Developer", MinSalary = 40000, MaxSalary = 70000 },
                new Job { JobTitle = "Analyst", MinSalary = 35000, MaxSalary = 60000 }
            });
            _context.SaveChanges();

            // Act
            var result = _jobService.GetAllJobs();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Analyst", result[0].JobTitle);
            Assert.Equal("Developer", result[1].JobTitle);
            Assert.Equal("Manager", result[2].JobTitle);
        }

        [Fact]
        public void AddJob_WithValidData_ReturnsNewJob()
        {
            // Act
            var result = _jobService.AddJob("Software Engineer", 45000, 75000);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Software Engineer", result.JobTitle);
            Assert.Equal(45000, result.MinSalary);
            Assert.Equal(75000, result.MaxSalary);
            Assert.True(result.JobId > 0);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void AddJob_WithEmptyTitle_ThrowsArgumentException(string title)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => _jobService.AddJob(title, 40000, 60000));
            Assert.Equal("Job title cannot be empty.", exception.Message);
        }

        [Fact]
        public void AddJob_WithNegativeMinSalary_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => _jobService.AddJob("Test Job", -1000, 50000));
            Assert.Equal("Minimum salary cannot be negative.", exception.Message);
        }

        [Fact]
        public void AddJob_WithNegativeMaxSalary_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => _jobService.AddJob("Test Job", 40000, -50000));
            Assert.Equal("Maximum salary cannot be negative.", exception.Message);
        }

        [Fact]
        public void AddJob_WithMinSalaryGreaterThanMaxSalary_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => _jobService.AddJob("Test Job", 60000, 50000));
            Assert.Equal("Minimum salary cannot be greater than maximum salary.", exception.Message);
        }

        [Fact]
        public void UpdateJob_WithValidData_UpdatesJobSuccessfully()
        {
            // Arrange
            var job = new Job { JobTitle = "Original Title", MinSalary = 30000, MaxSalary = 50000 };
            _context.Jobs.Add(job);
            _context.SaveChanges();

            // Act
            var result = _jobService.UpdateJob(job.JobId, "Updated Title", 35000, 55000);

            // Assert
            Assert.Equal("Updated Title", result.JobTitle);
            Assert.Equal(35000, result.MinSalary);
            Assert.Equal(55000, result.MaxSalary);
        }

        [Fact]
        public void UpdateJob_WithNonExistentJobId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(
                () => _jobService.UpdateJob(999, "Test Title", 40000, 60000));
            Assert.Equal("Job with ID 999 not found.", exception.Message);
        }

        [Fact]
        public void DeleteJob_WithValidJobId_DeletesJobSuccessfully()
        {
            // Arrange
            var job = new Job { JobTitle = "Test Job", MinSalary = 30000, MaxSalary = 50000 };
            _context.Jobs.Add(job);
            _context.SaveChanges();

            // Act
            _jobService.DeleteJob(job.JobId);

            // Assert
            var deletedJob = _context.Jobs.Find(job.JobId);
            Assert.Null(deletedJob);
        }

        [Fact]
        public void DeleteJob_WithNonExistentJobId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(
                () => _jobService.DeleteJob(999));
            Assert.Equal("Job with ID 999 not found.", exception.Message);
        }

        [Fact]
        public void DeleteJob_WhenJobIsAssignedToEmployee_ThrowsInvalidOperationException()
        {
            // Arrange
            var job = new Job { JobTitle = "Test Job", MinSalary = 30000, MaxSalary = 50000 };
            _context.Jobs.Add(job);
            _context.SaveChanges();

            var employee = new Employee { JobId = job.JobId, FirstName = "John", LastName = "Doe" };
            _context.Employees.Add(employee);
            _context.SaveChanges();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => _jobService.DeleteJob(job.JobId));
            Assert.Equal("Cannot delete this job because it is currently assigned to one or more employees.", exception.Message);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
