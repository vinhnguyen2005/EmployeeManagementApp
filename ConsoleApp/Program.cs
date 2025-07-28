using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp.Models;



namespace ConsoleApp
{
    class Program
    {
            static void Main(string[] args)
            {
                using var context = new EmployeeContext();
                var jobService = new JobService(context);

                bool running = true;
                while (running)
                {
                    Console.Clear();
                    Console.WriteLine("--- Job Management Console ---");
                    Console.WriteLine("1. List All Jobs");
                    Console.WriteLine("2. Add New Job");
                    Console.WriteLine("3. Update Job");
                    Console.WriteLine("4. Delete Job");
                    Console.WriteLine("5. Exit");
                    Console.Write("Select an option: ");

                    switch (Console.ReadLine())
                    {
                        case "1":
                            ListAllJobs(jobService);
                            break;
                        case "2":
                            AddNewJob(jobService);
                            break;
                        case "3":
                            UpdateJob(jobService);
                            break;
                        case "4":
                            DeleteJob(jobService);
                            break;
                        case "5":
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Invalid option.");
                            break;
                    }
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }

            static void ListAllJobs(JobService service)
            {
                Console.WriteLine("\n--- All Jobs ---");
                var jobs = service.GetAllJobs();
                if (!jobs.Any())
                {
                    Console.WriteLine("No jobs found in the database.");
                    return;
                }
                foreach (var job in jobs)
                {
                    Console.WriteLine($"ID: {job.JobId}, Title: {job.JobTitle}, Salary Range: {job.MinSalary:C} - {job.MaxSalary:C}");
                }
            }

            static void AddNewJob(JobService service)
            {
                Console.WriteLine("\n--- Add New Job ---");
                Console.Write("Enter Job Title: ");
                string title = Console.ReadLine();

                decimal? minSalary = GetDecimalFromConsole("Enter Minimum Salary (leave blank if none): ");
                decimal? maxSalary = GetDecimalFromConsole("Enter Maximum Salary (leave blank if none): ");

                try
                {
                    var newJob = service.AddJob(title, minSalary, maxSalary);
                    Console.WriteLine($"Successfully added job: {newJob.JobTitle}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            static void UpdateJob(JobService service)
            {
                Console.WriteLine("\n--- Update Job ---");

                ListAllJobs(service);

                Console.Write("\nEnter Job ID to update: ");
                if (int.TryParse(Console.ReadLine(), out int jobId))
                {
                    Console.Write("Enter New Job Title: ");
                    string newTitle = Console.ReadLine();

                    decimal? newMinSalary = GetDecimalFromConsole("Enter New Minimum Salary (leave blank if none): ");
                    decimal? newMaxSalary = GetDecimalFromConsole("Enter New Maximum Salary (leave blank if none): ");

                    try
                    {
                        var updatedJob = service.UpdateJob(jobId, newTitle, newMinSalary, newMaxSalary);
                        Console.WriteLine($"Successfully updated job: {updatedJob.JobTitle}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Job ID.");
                }
            }

            static void DeleteJob(JobService service)
            {
                Console.WriteLine("\n--- Delete Job ---");

                ListAllJobs(service);

                Console.Write("\nEnter Job ID to delete: ");
                if (int.TryParse(Console.ReadLine(), out int jobId))
                {
                    try
                    {
                        service.DeleteJob(jobId);
                        Console.WriteLine($"Successfully deleted job with ID: {jobId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Job ID.");
                }
            }
            static decimal? GetDecimalFromConsole(string prompt)
            {
                while (true)
                {
                    Console.Write(prompt);
                    string input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        return null;
                    }

                    if (decimal.TryParse(input, out decimal result))
                    {
                        return result;
                    }
                    Console.WriteLine("Invalid input. Please enter a valid number or leave it blank.");
                }
            }
        }

    }
