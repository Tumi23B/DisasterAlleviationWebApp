using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Disaster_Alleviation_Web_App.Data;
using Disaster_Alleviation_Web_App.Models;

namespace DisasterAlleviation.LoadStressTests
{
    class Program
    {
        private static ApplicationDbContext context;
        private static readonly Random random = new Random();
        private static readonly List<OperationMetric> operationMetrics = new List<OperationMetric>();
        private static readonly object metricsLock = new object();

        static async Task Main(string[] args)
        {
            DisplayHeader();

            // --- Setup DbContext ---
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Disaster_Alleviation_Web_App;Trusted_Connection=True;MultipleActiveResultSets=true")
                .Options;

            context = new ApplicationDbContext(options);

            try
            {
                context.Database.EnsureCreated();
                Console.WriteLine("✓ Database connection established and verified\n");
            }
            catch (Exception ex)
            {
                DisplayError($"Database connection failed: {ex.Message}");
                return;
            }

            // --- Ensure default users exist ---
            Console.WriteLine("📊 Setting up test environment...");
            var usersList = await EnsureDefaultUsers(500);

            // --- Run Load Test (Moderate Traffic) ---
            DisplayTestHeader("LOAD TEST", "Moderate Traffic Simulation");
            var loadResults = await RunTestWithLiveDashboard(usersList, users: 50, actionsPerUser: 10, testName: "Load Test");

            // --- Run Stress Test (Extreme Traffic) ---
            DisplayTestHeader("STRESS TEST", "Extreme Traffic Simulation");
            var stressResults = await RunTestWithLiveDashboard(usersList, users: 200, actionsPerUser: 20, testName: "Stress Test");

            // --- Display Comprehensive Results ---
            DisplayComprehensiveResults(loadResults, stressResults);

            // --- Display Slow Operations Analysis ---
            DisplaySlowOperationsAnalysis();

            // --- Performance Analysis ---
            DisplayPerformanceAnalysis(loadResults, stressResults);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n🎉 All tests completed successfully! Press any key to exit...");
            Console.ResetColor();
            Console.ReadLine();
        }

        /// <summary>
        /// Display application header
        /// </summary>
        private static void DisplayHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('═', 70));
            Console.WriteLine("🚀 DISASTER ALLEVIATION - DATABASE LOAD & STRESS TEST SIMULATOR");
            Console.WriteLine(new string('═', 70));
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Display test section header
        /// </summary>
        private static void DisplayTestHeader(string testType, string description)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\n{new string('─', 50)}");
            Console.WriteLine($"🧪 {testType}: {description}");
            Console.WriteLine(new string('─', 50));
            Console.ResetColor();
        }

        /// <summary>
        /// Display error message
        /// </summary>
        private static void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ ERROR: {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Ensure a minimum number of default users exist in the database
        /// </summary>
        private static async Task<List<ApplicationUser>> EnsureDefaultUsers(int minUsers)
        {
            Console.Write("🔧 Verifying test users in database... ");

            var users = await context.Users.OrderBy(u => u.Id).Take(minUsers).ToListAsync();
            var newUsersCount = 0;

            // Create new users if needed
            for (int i = users.Count; i < minUsers; i++)
            {
                var user = new ApplicationUser
                {
                    FullName = $"Auto User {i + 1}",
                    Email = $"auto{i + 1}@example.com",
                    UserName = $"auto{i + 1}@example.com",
                    Address = $"Generated Address {i + 1}",
                    Availability = "Weekends",
                    CellphoneNumber = $"07100000{i + 1:D3}",
                    Skills = "Testing"
                };
                context.Users.Add(user);
                users.Add(user);
                newUsersCount++;
            }

            if (newUsersCount > 0)
            {
                await context.SaveChangesAsync();
            }

            Console.WriteLine($"✓ {users.Count} users available ({newUsersCount} new users created)");
            return users;
        }

        /// <summary>
        /// Enhanced test runner with live dashboard and progress spinner
        /// </summary>
        private static async Task<TestResult> RunTestWithLiveDashboard(List<ApplicationUser> usersList, int users, int actionsPerUser, string testName)
        {
            var stopwatch = Stopwatch.StartNew();
            int totalActions = 0;
            int failedActions = 0;
            int donationsCreated = 0;
            int tasksCreated = 0;

            Console.WriteLine($"🚀 Starting {testName} with {users} concurrent users...");
            Console.WriteLine($"📈 Target: {actionsPerUser} actions per user ({users * actionsPerUser} total operations)\n");

            var tasks = new Task[users];
            var cancellationTokenSource = new CancellationTokenSource();

            // Start live dashboard
            var dashboardTask = Task.Run(() => DisplayLiveDashboard(
                testName, users, actionsPerUser,
                () => totalActions, () => failedActions,
                () => donationsCreated, () => tasksCreated,
                stopwatch, cancellationTokenSource.Token));

            for (int i = 0; i < users; i++)
            {
                int userIndex = i; // Capture for closure
                tasks[i] = Task.Run(async () =>
                {
                    for (int j = 0; j < actionsPerUser; j++)
                    {
                        var operationStopwatch = Stopwatch.StartNew();
                        var operationType = "";

                        try
                        {
                            // Pick a random user
                            var user = usersList[random.Next(usersList.Count)];

                            // Randomly choose action: Donation or Task
                            if (random.Next(0, 2) == 0)
                            {
                                operationType = "DONATION";
                                // Create Donation
                                var donation = new Donation
                                {
                                    DonorId = user.Id,
                                    Amount = random.Next(10, 500),
                                    DonationDate = DateTime.Now,
                                    Status = "Pending",
                                    DonationType = "Money",
                                    Description = "Load/Stress Test Donation"
                                };

                                await ExecuteWithLock(async () =>
                                {
                                    context.Add(donation);
                                    await context.SaveChangesAsync();
                                });

                                Interlocked.Increment(ref donationsCreated);
                            }
                            else
                            {
                                operationType = "TASK";
                                // Create Task
                                var taskItem = new HelperTasks
                                {
                                    HelperId = user.Id,
                                    Title = $"Load Test Task {Guid.NewGuid()}",
                                    TaskType = "Volunteer",
                                    Description = "Load/Stress Test Task",
                                    AssignedDate = DateTime.Now,
                                    Status = "Assigned"
                                };

                                await ExecuteWithLock(async () =>
                                {
                                    context.Add(taskItem);
                                    await context.SaveChangesAsync();
                                });

                                Interlocked.Increment(ref tasksCreated);
                            }

                            operationStopwatch.Stop();

                            // Record successful operation metric
                            lock (metricsLock)
                            {
                                operationMetrics.Add(new OperationMetric
                                {
                                    TestName = testName,
                                    OperationType = operationType,
                                    DurationMs = operationStopwatch.Elapsed.TotalMilliseconds,
                                    Timestamp = DateTime.Now
                                });
                            }

                            Interlocked.Increment(ref totalActions);
                        }
                        catch (Exception ex)
                        {
                            operationStopwatch.Stop();
                            Interlocked.Increment(ref failedActions);
                            Interlocked.Increment(ref totalActions);

                            // Record failed operation metric
                            lock (metricsLock)
                            {
                                operationMetrics.Add(new OperationMetric
                                {
                                    TestName = testName,
                                    OperationType = operationType,
                                    DurationMs = operationStopwatch.Elapsed.TotalMilliseconds,
                                    Timestamp = DateTime.Now,
                                    IsFailed = true,
                                    ErrorMessage = ex.Message
                                });
                            }
                        }
                    }
                });
            }

            await Task.WhenAll(tasks);
            cancellationTokenSource.Cancel();
            await dashboardTask;

            stopwatch.Stop();

            // Clear the dashboard line
            Console.Write(new string(' ', Console.WindowWidth));
            Console.Write("\r");

            Console.WriteLine($"✅ {testName} completed in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            Console.WriteLine($"   📊 Success: {totalActions - failedActions} | ❌ Failed: {failedActions}");
            Console.WriteLine($"   💰 Donations: {donationsCreated} | 👥 Tasks: {tasksCreated}");

            return new TestResult
            {
                TestName = testName,
                Users = users,
                ActionsPerUser = actionsPerUser,
                TotalActions = totalActions,
                FailedActions = failedActions,
                TotalTimeSeconds = stopwatch.Elapsed.TotalSeconds,
                AvgTimePerActionMs = stopwatch.Elapsed.TotalMilliseconds / totalActions,
                DonationsCreated = donationsCreated,
                TasksCreated = tasksCreated
            };
        }

        /// <summary>
        /// Display real-time live dashboard during test execution
        /// </summary>
        private static async Task DisplayLiveDashboard(string testName, int totalUsers, int actionsPerUser,
            Func<int> totalActions, Func<int> failedActions, Func<int> donationsCreated, Func<int> tasksCreated,
            Stopwatch stopwatch, CancellationToken cancellationToken)
        {
            var spinner = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
            var spinnerIndex = 0;
            var lastTotalActions = 0;
            var lastUpdateTime = DateTime.Now;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var currentTotalActions = totalActions();
                    var currentFailed = failedActions();
                    var currentDonations = donationsCreated();
                    var currentTasks = tasksCreated();
                    var elapsed = stopwatch.Elapsed;
                    var actionsPerSecond = currentTotalActions > 0 ? currentTotalActions / elapsed.TotalSeconds : 0;

                    // Calculate progress
                    var totalExpectedActions = totalUsers * actionsPerUser;
                    var progressPercentage = totalExpectedActions > 0 ? (double)currentTotalActions / totalExpectedActions * 100 : 0;

                    // Create progress bar
                    var progressBarWidth = 20;
                    var progressBarFilled = (int)(progressPercentage / 100 * progressBarWidth);
                    var progressBar = new string('█', progressBarFilled) + new string('░', progressBarWidth - progressBarFilled);

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"\r{spinner[spinnerIndex]} ");
                    Console.ResetColor();
                    Console.Write($"{testName} | ");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"[{progressBar}] {progressPercentage:F1}% ");
                    Console.ResetColor();

                    Console.Write($"| 📊 {currentTotalActions}/{totalExpectedActions} ");
                    Console.Write($"| ⚡ {actionsPerSecond:F1} ops/s ");
                    Console.Write($"| ❌ {currentFailed} ");
                    Console.Write($"| 💰 {currentDonations} ");
                    Console.Write($"| 👥 {currentTasks} ");
                    Console.Write($"| ⏱ {elapsed:mm\\:ss} ");

                    // Update spinner
                    spinnerIndex = (spinnerIndex + 1) % spinner.Length;
                    lastTotalActions = currentTotalActions;
                    lastUpdateTime = DateTime.Now;

                    await Task.Delay(100, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Execute database operations with thread safety
        /// </summary>
        private static async Task ExecuteWithLock(Func<Task> operation)
        {
            lock (context)
            {
                var task = operation();
                task.Wait();
            }
        }

        /// <summary>
        /// Display slow operations analysis
        /// </summary>
        private static void DisplaySlowOperationsAnalysis()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n" + new string('═', 80));
            Console.WriteLine("🐌 SLOW OPERATIONS ANALYSIS - TOP 10 LONGEST RUNNING OPERATIONS");
            Console.WriteLine(new string('═', 80));
            Console.ResetColor();

            var slowOperations = operationMetrics
                .OrderByDescending(m => m.DurationMs)
                .Take(10)
                .ToList();

            if (!slowOperations.Any())
            {
                Console.WriteLine("No operation metrics recorded.");
                return;
            }

            Console.WriteLine($"{"TEST",-12} {"TYPE",-10} {"DURATION",-12} {"STATUS",-8} {"TIMESTAMP"}");
            Console.WriteLine(new string('─', 80));

            foreach (var op in slowOperations)
            {
                var status = op.IsFailed ? "❌ FAIL" : "✅ OK";
                var color = op.DurationMs > 1000 ? ConsoleColor.Red :
                           op.DurationMs > 500 ? ConsoleColor.Yellow : ConsoleColor.Green;

                Console.ForegroundColor = color;
                Console.WriteLine($"{op.TestName,-12} {op.OperationType,-10} {op.DurationMs,8:F2}ms {status,-8} {op.Timestamp:HH:mm:ss.fff}");
                Console.ResetColor();
            }

            // Summary statistics
            var avgDuration = operationMetrics.Average(m => m.DurationMs);
            var maxDuration = operationMetrics.Max(m => m.DurationMs);
            var slowOperationsCount = operationMetrics.Count(m => m.DurationMs > 1000);

            Console.WriteLine(new string('─', 80));
            Console.WriteLine($"📈 Summary: Avg: {avgDuration:F2}ms | Max: {maxDuration:F2}ms | Slow (>1s): {slowOperationsCount} operations");
        }

        /// <summary>
        /// Displays comprehensive test results
        /// </summary>
        private static void DisplayComprehensiveResults(TestResult load, TestResult stress)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n" + new string('═', 100));
            Console.WriteLine("📊 COMPREHENSIVE TEST RESULTS SUMMARY");
            Console.WriteLine(new string('═', 100));
            Console.ResetColor();

            // Header
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{"TEST TYPE",-15} {"USERS",-8} {"ACTIONS/USER",-15} {"TOTAL ACTIONS",-15} {"SUCCESS RATE",-12} {"FAILED",-8} {"TOTAL TIME",-12} {"AVG TIME/ACTION",-15}");
            Console.WriteLine(new string('─', 100));
            Console.ResetColor();

            // Load Test Results
            DisplayResultRow(load, "🟢 LOAD TEST");

            // Stress Test Results  
            DisplayResultRow(stress, "🔴 STRESS TEST");

            Console.WriteLine(new string('─', 100));
        }

        /// <summary>
        /// Display individual result row
        /// </summary>
        private static void DisplayResultRow(TestResult result, string testLabel)
        {
            var successRate = ((double)(result.TotalActions - result.FailedActions) / result.TotalActions * 100);
            var successColor = successRate >= 95 ? ConsoleColor.Green :
                             successRate >= 85 ? ConsoleColor.Yellow : ConsoleColor.Red;

            Console.Write($"{testLabel,-15} {result.Users,-8} {result.ActionsPerUser,-15} {result.TotalActions,-15} ");

            Console.ForegroundColor = successColor;
            Console.Write($"{successRate:F1}%".PadRight(12));
            Console.ResetColor();

            Console.Write($"{result.FailedActions,-8} {result.TotalTimeSeconds,-12:F2}s {result.AvgTimePerActionMs,-15:F2}ms");

            // Additional metrics
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" 💰{result.DonationsCreated} 👥{result.TasksCreated}");
            Console.ResetColor();

            Console.WriteLine();
        }

        /// <summary>
        /// Display performance analysis and recommendations
        /// </summary>
        private static void DisplayPerformanceAnalysis(TestResult load, TestResult stress)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\n" + new string('═', 80));
            Console.WriteLine("📈 PERFORMANCE ANALYSIS & RECOMMENDATIONS");
            Console.WriteLine(new string('═', 80));
            Console.ResetColor();

            var loadSuccessRate = ((double)(load.TotalActions - load.FailedActions) / load.TotalActions * 100);
            var stressSuccessRate = ((double)(stress.TotalActions - stress.FailedActions) / stress.TotalActions * 100);

            // Performance Metrics
            Console.WriteLine("📊 Key Metrics:");
            Console.WriteLine($"   • Load Test Success Rate: {GetPerformanceIndicator(loadSuccessRate)} {loadSuccessRate:F1}%");
            Console.WriteLine($"   • Stress Test Success Rate: {GetPerformanceIndicator(stressSuccessRate)} {stressSuccessRate:F1}%");
            Console.WriteLine($"   • Performance Scaling: {CalculateScalingFactor(load, stress):F2}x");
            Console.WriteLine($"   • Total Database Operations: {load.TotalActions + stress.TotalActions}");

            // Database Performance Insights
            var slowOps = operationMetrics.Where(m => m.DurationMs > 1000).ToList();
            if (slowOps.Any())
            {
                Console.WriteLine($"   • Slow Operations (>1s): {slowOps.Count}");
                Console.WriteLine($"   • Worst Performance: {slowOps.Max(m => m.DurationMs):F2}ms");
            }

            // Recommendations
            Console.WriteLine("\n💡 Recommendations:");

            if (stressSuccessRate < 90)
            {
                Console.WriteLine("   ⚠️  High failure rate under stress - consider:");
                Console.WriteLine("      • Database connection pooling optimization");
                Console.WriteLine("      • Implementing request queuing mechanisms");
                Console.WriteLine("      • Scaling database resources");
            }

            if (stress.AvgTimePerActionMs > load.AvgTimePerActionMs * 2)
            {
                Console.WriteLine("   ⚠️  Significant performance degradation under load - consider:");
                Console.WriteLine("      • Query optimization and indexing");
                Console.WriteLine("      • Caching strategies");
                Console.WriteLine("      • Database read replicas for heavy read scenarios");
            }

            if (loadSuccessRate >= 95 && stressSuccessRate >= 85)
            {
                Console.WriteLine("   ✅ System shows good resilience under tested conditions");
                Console.WriteLine("   ✅ Consider gradual scaling with monitoring");
            }

            // Additional insights based on your actual results
            if (stress.AvgTimePerActionMs < load.AvgTimePerActionMs)
            {
                Console.WriteLine("   🎉 Excellent! System performs better under stress - indicates good scalability");
            }
        }

        /// <summary>
        /// Get performance indicator emoji
        /// </summary>
        private static string GetPerformanceIndicator(double successRate)
        {
            return successRate >= 95 ? "✅" :
                   successRate >= 85 ? "⚠️ " : "❌";
        }

        /// <summary>
        /// Calculate performance scaling factor
        /// </summary>
        private static double CalculateScalingFactor(TestResult load, TestResult stress)
        {
            var loadThroughput = load.TotalActions / load.TotalTimeSeconds;
            var stressThroughput = stress.TotalActions / stress.TotalTimeSeconds;
            return stressThroughput / loadThroughput;
        }
    }

    /// <summary>
    /// Enhanced helper class to store comprehensive test results
    /// </summary>
    class TestResult
    {
        public string TestName { get; set; } = string.Empty;
        public int Users { get; set; }
        public int ActionsPerUser { get; set; }
        public int TotalActions { get; set; }
        public int FailedActions { get; set; }
        public double TotalTimeSeconds { get; set; }
        public double AvgTimePerActionMs { get; set; }
        public int DonationsCreated { get; set; }
        public int TasksCreated { get; set; }
    }

    /// <summary>
    /// Class to track individual operation metrics
    /// </summary>
    class OperationMetric
    {
        public string TestName { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public double DurationMs { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFailed { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}