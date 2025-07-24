using ConsoleApp.Models;
using Microsoft.EntityFrameworkCore;
using ConsoleApp;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit; 

namespace UnitTest
{
    public class JobServiceTest
    {
        private readonly EmployeeContext _context;
        private readonly JobService _jobService;

        public JobServiceTest()
        {
            var options = new DbContextOptionsBuilder<EmployeeContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EmployeeContext(options);
            _jobService = new JobService(_context);
        }

        #region Create (AddJob) Tests

        [Fact] 
        public void AddJob_ShouldAddJob_WhenDataIsValid()
        {
            // Arrange
            string title = "Test Developer";
            decimal minSalary = 5000;
            decimal maxSalary = 10000;

            // Act
            _jobService.AddJob(title, minSalary, maxSalary);

            Assert.Equal(1, _context.Jobs.Count()); 
            Assert.Equal(title, _context.Jobs.First().JobTitle);
            Assert.Equal(minSalary, _context.Jobs.First().MinSalary);
        }

        public void AddJob_ShouldThrowException_WhenTitleIsEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                string title = ""; 
                _jobService.AddJob(title, 5000, 10000);
            });
        }

        [Fact]
        public void AddJob_ShouldThrowException_WhenMinSalaryIsNegative()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                decimal minSalary = -100;
                _jobService.AddJob("Negative Salary Job", minSalary, 10000);
            });
        }

        [Fact] 
        public void AddJob_ShouldThrowException_WhenMinSalaryIsGreaterThanMax()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                string title = "Invalid Job";
                decimal minSalary = 10000;
                decimal maxSalary = 5000;
                _jobService.AddJob(title, minSalary, maxSalary);
            });
        }

        #endregion

        #region Read (GetAllJobs) Tests

        [Fact]
        public void GetAllJobs_ShouldReturnEmptyList_WhenDbIsEmpty()
        {
            var result = _jobService.GetAllJobs();
            Assert.NotNull(result); 
            Assert.Empty(result); 
        }

        [Fact]
        public void GetAllJobs_ShouldReturnAllJobs_SortedByTitle()
        {
            _jobService.AddJob("Z-Job", null, null);
            _jobService.AddJob("A-Job", null, null);
            _jobService.AddJob("M-Job", null, null);
            var result = _jobService.GetAllJobs();

            Assert.Equal(3, result.Count);
            Assert.Equal("A-Job", result[0].JobTitle);
            Assert.Equal("M-Job", result[1].JobTitle);
            Assert.Equal("Z-Job", result[2].JobTitle);
        }

        #endregion

        #region Update (UpdateJob) Tests

        [Fact] 
        public void UpdateJob_ShouldUpdateJob_WhenDataIsValid()
        {
            var job = _jobService.AddJob("Old Title", 1000, 2000);
            string newTitle = "New Title";
            decimal newMinSalary = 1500;
            decimal newMaxSalary = 2500;

            _jobService.UpdateJob(job.JobId, newTitle, newMinSalary, newMaxSalary);
            var updatedJob = _context.Jobs.Find(job.JobId);

            Assert.NotNull(updatedJob);
            Assert.Equal(newTitle, updatedJob.JobTitle);
            Assert.Equal(newMinSalary, updatedJob.MinSalary);
        }

        [Fact]
        public void UpdateJob_ShouldThrowException_WhenJobIdDoesNotExist()
        {
            Assert.Throws<KeyNotFoundException>(() =>
            {
                _jobService.UpdateJob(999, "Non-existent", 0, 0);
            });
        }

        #endregion

        #region Delete (DeleteJob) Tests

        [Fact]
        public void DeleteJob_ShouldDeleteJob_WhenJobIsNotInUse()
        {
            var job = _jobService.AddJob("Temporary Job", null, null);
            Assert.Equal(1, _context.Jobs.Count());
            _jobService.DeleteJob(job.JobId);
            Assert.Equal(0, _context.Jobs.Count());
        }

        [Fact] 
        public void DeleteJob_ShouldThrowException_WhenJobIsInUse()
        {
            var job = _jobService.AddJob("Accountant", 4000, 9000);
            _context.Employees.Add(new Employee
            {
                LastName = "Test",
                Email = "test@test.com",
                HireDate = DateOnly.FromDateTime(DateTime.Today),
                Salary = 5000,
                JobId = job.JobId
            });
            _context.SaveChanges();

            Assert.Throws<InvalidOperationException>(() => _jobService.DeleteJob(job.JobId));
        }

        [Fact] // Thay thế [TestMethod]
        public void DeleteJob_ShouldThrowException_WhenJobIdDoesNotExist()
        {
            Assert.Throws<KeyNotFoundException>(() => _jobService.DeleteJob(999));
        }

        #endregion
    }
}