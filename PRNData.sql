USE [employee]
GO

-- Insert data into regions
INSERT INTO [dbo].[regions] ([region_name]) VALUES
('North America'),
('Europe'),
('Asia');

-- Insert data into countries
INSERT INTO [dbo].[countries] ([country_id], [country_name], [region_id]) VALUES
('US', 'United States', 1),
('UK', 'United Kingdom', 2),
('CN', 'China', 3);

-- Insert data into locations
INSERT INTO [dbo].[locations] ([street_address], [postal_code], [city], [state_province], [country_id]) VALUES
('123 Main St', '12345', 'New York', 'NY', 'US'),
('456 High St', 'SW1A', 'London', NULL, 'UK'),
('789 Beijing Rd', '100000', 'Beijing', NULL, 'CN');

-- Insert data into departments
INSERT INTO [dbo].[departments] ([department_name], [location_id]) VALUES
('IT', 1),
('HR', 2),
('Finance', 3);

-- Insert data into jobs
INSERT INTO [dbo].[jobs] ([job_title], [min_salary], [max_salary]) VALUES
('Software Engineer', 50000.00, 100000.00),
('HR Manager', 45000.00, 90000.00),
('Accountant', 40000.00, 80000.00);

-- Insert data into employees
INSERT INTO [dbo].[employees] ([first_name], [last_name], [email], [phone_number], [hire_date], [job_id], [salary], [manager_id], [department_id], [IsActive], [ProfilePicturePath], [FaceEncoding]) VALUES
('John', 'Doe', 'john.doe@company.com', '123-456-7890', '2023-01-15', 1, 75000.00, NULL, 1, 1, '/images/john_doe.jpg', 0x123456),
('Jane', 'Smith', 'jane.smith@company.com', '987-654-3210', '2022-06-20', 2, 80000.00, 1, 2, 1, '/images/jane_smith.jpg', 0x789012),
('Bob', 'Johnson', 'bob.johnson@company.com', NULL, '2021-03-10', 3, 60000.00, 1, 3, 1, NULL, NULL);

-- Insert data into dependents
INSERT INTO [dbo].[dependents] ([first_name], [last_name], [relationship], [employee_id]) VALUES
('Alice', 'Doe', 'Spouse', 1),
('Tom', 'Doe', 'Child', 1),
('Emma', 'Smith', 'Child', 2);

-- Insert data into roles
INSERT INTO [dbo].[roles] ([role_name]) VALUES
('Admin'),
('Employee'),
('Manager');

-- Insert data into users
INSERT INTO [dbo].[users] ([username], [password_hash], [employee_id], [IsLocked]) VALUES
('johndoe', 'hashed_password_123', 1, 0),
('janesmith', 'hashed_password_456', 2, 0),
('bobjohnson', 'hashed_password_789', 3, 0);

-- Insert data into user_roles
INSERT INTO [dbo].[user_roles] ([user_id], [role_id]) VALUES
(1, 1), -- John Doe as Admin
(1, 3), -- John Doe as Manager
(2, 2), -- Jane Smith as Employee
(3, 2); -- Bob Johnson as Employee

-- Insert data into tasks
INSERT INTO [dbo].[tasks] ([employee_id], [task_description], [deadline], [status], [priority], [performance_score], [review_comment]) VALUES
(1, 'Develop new feature', '2025-08-01', 'Pending', 'High', NULL, NULL),
(2, 'Conduct employee training', '2025-07-30', 'In Progress', 'Medium', 85.50, 'Good performance'),
(3, 'Prepare financial report', '2025-07-25', 'Completed', 'Low', 90.00, 'Excellent work');

-- Insert data into requests
INSERT INTO [dbo].[requests] ([employee_id], [manager_id], [request_type], [description], [status], [raise_amount], [StartDate], [EndDate]) VALUES
(2, 1, 'Salary Increase', 'Request for 10% salary increase due to performance', 'Pending', 8000.00, NULL, NULL),
(3, 1, 'Leave', 'Request for 5 days leave', 'Approved', NULL, '2025-08-01', '2025-08-05');

-- Insert data into AttendanceLogs
INSERT INTO [dbo].[AttendanceLogs] ([EmployeeId], [CheckInTime], [SimilarityScore], [Status]) VALUES
(1, '2025-07-24 08:00:00', 0.95, 'Checked In'),
(2, '2025-07-24 08:15:00', 0.92, 'Checked In'),
(3, '2025-07-24 08:30:00', 0.90, 'Checked In');

-- Insert data into AuditLogs
INSERT INTO [dbo].[AuditLogs] ([UserId], [Username], [ActionType], [Details], [Timestamp]) VALUES
(1, 'johndoe', 'Login', 'User logged in successfully', '2025-07-24 08:00:00'),
(2, 'janesmith', 'Request Submission', 'Submitted salary increase request', '2025-07-24 09:00:00'),
(3, 'bobjohnson', 'Task Completion', 'Completed financial report task', '2025-07-24 10:00:00');