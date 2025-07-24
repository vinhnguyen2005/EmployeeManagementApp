using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp.Models;

namespace ConsoleApp
{ 

    class JobService
    {
        private readonly EmployeeContext _context;

        public JobService(EmployeeContext context)
        {
            _context = context;
        }

        public List<Job> GetAllJobs()
        {
            return _context.Jobs.OrderBy(j => j.JobTitle).ToList();
        }
        public Job AddJob(string title, decimal? minSalary, decimal? maxSalary)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Job title cannot be empty.");
            }
            if (minSalary.HasValue && minSalary < 0)
            {
                throw new ArgumentException("Minimum salary cannot be negative.");
            }
            if (maxSalary.HasValue && maxSalary < 0)
            {
                throw new ArgumentException("Maximum salary cannot be negative.");
            }
            if (minSalary.HasValue && maxSalary.HasValue && minSalary > maxSalary)
            {
                throw new ArgumentException("Minimum salary cannot be greater than maximum salary.");
            }

            var newJob = new Job
            {
                JobTitle = title,
                MinSalary = minSalary,
                MaxSalary = maxSalary
            };

            _context.Jobs.Add(newJob);
            _context.SaveChanges();
            return newJob;
        }

        public Job UpdateJob(int jobId, string newTitle, decimal? newMinSalary, decimal? newMaxSalary)
        {
            var jobToUpdate = _context.Jobs.Find(jobId);
            if (jobToUpdate == null)
            {
                throw new KeyNotFoundException($"Job with ID {jobId} not found.");
            }
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                throw new ArgumentException("Job title cannot be empty.");
            }
            if (newMinSalary.HasValue && newMinSalary < 0)
            {
                throw new ArgumentException("Minimum salary cannot be negative.");
            }
            if (newMaxSalary.HasValue && newMaxSalary < 0)
            {
                throw new ArgumentException("Maximum salary cannot be negative.");
            }
            if (newMinSalary.HasValue && newMaxSalary.HasValue && newMinSalary > newMaxSalary)
            {
                throw new ArgumentException("Minimum salary cannot be greater than maximum salary.");
            }
            jobToUpdate.JobTitle = newTitle;
            jobToUpdate.MinSalary = newMinSalary;
            jobToUpdate.MaxSalary = newMaxSalary;
            _context.SaveChanges();
            return jobToUpdate;
        }

        public void DeleteJob(int jobId)
        {
            var jobToDelete = _context.Jobs.Find(jobId);
            if (jobToDelete == null)
            {
                throw new KeyNotFoundException($"Job with ID {jobId} not found.");
            }

            bool isJobInUse = _context.Employees.Any(e => e.JobId == jobId);
            if (isJobInUse)
            {
                throw new InvalidOperationException("Cannot delete this job because it is currently assigned to one or more employees.");
            }

            _context.Jobs.Remove(jobToDelete);
            _context.SaveChanges();
        }
    }
    

}
