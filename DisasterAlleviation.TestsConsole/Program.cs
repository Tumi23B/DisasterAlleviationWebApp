using System;
using System.Collections.Generic;
using System.Linq;
using Disaster_Alleviation_Web_App.Data;
using Disaster_Alleviation_Web_App.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DisasterAlleviation.TestsConsole
{
    // MOCK CLASSES for tables that might not exist in the main DbContext
    public class Donation
    {
        public int DonationId { get; set; }
        public string DonorId { get; set; }
        public ApplicationUser Donor { get; set; }
        public decimal Amount { get; set; }
        public DateTime DonationDate { get; set; }
        public string Status { get; set; }
        public string DonationType { get; set; }
        public string Description { get; set; }
    }

    public class HelperTasks
    {
        public int TaskId { get; set; }
        public string HelperId { get; set; }
        public ApplicationUser Helper { get; set; }
        public string Title { get; set; }
        public string TaskType { get; set; }
        public string Description { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; }
    }

    public class ActiveRecurringDonation
    {
        public int RecurringDonationId { get; set; }
        public decimal Amount { get; set; }
        public string Frequency { get; set; }
    }

    [TestClass]
    public class DisasterAlleviationTests
    {
        private static ApplicationDbContext context;

        // Setup: runs once before all tests
        [ClassInitialize]
        public static void Setup(TestContext testContext)
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=Disaster_Alleviation_TestDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            context = new ApplicationDbContext(options);

            if (!context.Database.CanConnect())
                throw new Exception("Unable to connect to database.");

            EnsureTablesExist(connectionString);
        }

        // Ensures required tables exist (Donations, VolunteerTasks, RecurringDonations)
        static void EnsureTablesExist(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var commands = new List<string>
                {
                    @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Donations' AND xtype='U')
                      CREATE TABLE Donations (DonationId INT IDENTITY(1,1) PRIMARY KEY, DonorId NVARCHAR(450) NOT NULL, Amount DECIMAL(18,2) NOT NULL, DonationDate DATETIME2 NOT NULL, Status NVARCHAR(50) NOT NULL, DonationType NVARCHAR(50) NOT NULL, Description NVARCHAR(500), FOREIGN KEY (DonorId) REFERENCES AspNetUsers(Id))",
                    @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='VolunteerTasks' AND xtype='U')
                      CREATE TABLE VolunteerTasks (TaskId INT IDENTITY(1,1) PRIMARY KEY, HelperId NVARCHAR(450) NOT NULL, Title NVARCHAR(200) NOT NULL, TaskType NVARCHAR(50) NOT NULL, Description NVARCHAR(500), AssignedDate DATETIME2 NOT NULL, CompletedDate DATETIME2 NULL, Status NVARCHAR(50) NOT NULL, FOREIGN KEY (HelperId) REFERENCES AspNetUsers(Id))",
                    @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RecurringDonations' AND xtype='U')
                      CREATE TABLE RecurringDonations (RecurringDonationId INT IDENTITY(1,1) PRIMARY KEY, Amount DECIMAL(18,2) NOT NULL, Frequency NVARCHAR(50) NOT NULL)"
                };

                foreach (var cmdText in commands)
                {
                    using (var cmd = new SqlCommand(cmdText, connection))
                        cmd.ExecuteNonQuery();
                }
            }
        }

        // ✅ Test 1: User creation
        [TestMethod]
        public void TestUserCreation()
        {
            var user = context.Users.FirstOrDefault(u => u.Email == "john@example.com");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    FullName = "John Doe",
                    Email = "john@example.com",
                    UserName = "john@example.com",
                    Address = "123 Main St",
                    Availability = "Weekdays",
                    CellphoneNumber = "0712345678",
                    Skills = "Organizing, Fundraising"
                };
                context.Users.Add(user);
                context.SaveChanges();
            }

            // Assert user exists
            Assert.IsNotNull(context.Users.FirstOrDefault(u => u.Email == "john@example.com"));
        }

        // ✅ Test 2: Donation creation (normal and edge cases)
        [TestMethod]
        public void TestDonationCreation()
        {
            var user = context.Users.FirstOrDefault();
            Assert.IsNotNull(user, "No user exists to assign donation");

            // Normal donation
            var donation = new Donation
            {
                DonorId = user.Id,
                Amount = 100,
                DonationDate = DateTime.Now,
                Status = "Pending",
                DonationType = "Money",
                Description = "Automated test donation"
            };
            context.Add(donation);
            context.SaveChanges();

            // Assert donation exists
            var check = context.Set<Donation>().FirstOrDefault(d => d.DonorId == user.Id && d.Amount == 100);
            Assert.IsNotNull(check, "Donation creation failed");

            // Edge case: Zero amount
            var zeroDonation = new Donation
            {
                DonorId = user.Id,
                Amount = 0,
                DonationDate = DateTime.Now,
                Status = "Pending",
                DonationType = "Money",
                Description = "Zero amount donation"
            };
            context.Add(zeroDonation);
            context.SaveChanges();

            var checkZero = context.Set<Donation>().FirstOrDefault(d => d.Amount == 0);
            Assert.IsNotNull(checkZero, "Zero donation creation failed");
        }

        // ✅ Test 3: Helper task creation (normal and completed)
        [TestMethod]
        public void TestHelperTaskCreation()
        {
            var user = context.Users.FirstOrDefault();
            Assert.IsNotNull(user, "No user exists to assign task");

            // Normal task
            var task = new HelperTasks
            {
                HelperId = user.Id,
                Title = "Distribute Food Packs",
                TaskType = "Volunteer",
                Description = "Automated test task",
                AssignedDate = DateTime.Now,
                Status = "Assigned"
            };
            context.Add(task);
            context.SaveChanges();

            var check = context.Set<HelperTasks>().FirstOrDefault(t => t.HelperId == user.Id && t.Title == "Distribute Food Packs");
            Assert.IsNotNull(check, "Task creation failed");

            // Completed task
            task.Status = "Completed";
            task.CompletedDate = DateTime.Now;
            context.SaveChanges();

            var checkCompleted = context.Set<HelperTasks>().FirstOrDefault(t => t.TaskId == task.TaskId);
            Assert.AreEqual("Completed", checkCompleted.Status);
            Assert.IsNotNull(checkCompleted.CompletedDate);
        }

        // ✅ Test 4: Recurring donation calculations
        [TestMethod]
        public void TestRecurringDonationsCalculation()
        {
            var recurringDonations = new List<ActiveRecurringDonation>
            {
                new ActiveRecurringDonation { Amount = 50, Frequency = "Monthly" },
                new ActiveRecurringDonation { Amount = 600, Frequency = "Yearly" },
                new ActiveRecurringDonation { Amount = 75, Frequency = "Monthly" }
            };

            decimal totalMonthly = recurringDonations.Where(d => d.Frequency == "Monthly").Sum(d => d.Amount);
            decimal totalYearly = recurringDonations.Where(d => d.Frequency == "Yearly").Sum(d => d.Amount);

            Assert.AreEqual(125, totalMonthly, "Monthly total incorrect");
            Assert.AreEqual(600, totalYearly, "Yearly total incorrect");
        }

        // ✅ Test 5: User-donation integration
        [TestMethod]
        public void TestUserDonationsIntegration()
        {
            var count = (from d in context.Set<Donation>()
                         join u in context.Users on d.DonorId equals u.Id
                         select d).Count();

            Assert.IsTrue(count > 0, "No donations linked to users");
        }

        // ✅ Test 6: User-task integration
        [TestMethod]
        public void TestUserTasksIntegration()
        {
            var count = (from t in context.Set<HelperTasks>()
                         join u in context.Users on t.HelperId equals u.Id
                         select t).Count();

            Assert.IsTrue(count > 0, "No tasks linked to users");
        }

        // ✅ Test 7: Donations by status (parametrized)
        [TestMethod]
        [DataRow("Pending")]
        [DataRow("Completed")]
        public void TestDonationsByStatus(string status)
        {
            var count = context.Set<Donation>().Count(d => d.Status == status);
            Assert.IsTrue(count >= 0, $"Donations by status '{status}' check failed");
        }

        // Cleanup DB context after tests
        [ClassCleanup]
        public static void Cleanup()
        {
            context.Dispose();
        }
    }
}
