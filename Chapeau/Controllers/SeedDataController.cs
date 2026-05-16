using Chapeau.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Chapeau.Controllers
{
    /// <summary>
    /// Seed data controller for development/testing purposes.
    /// Creates test employee accounts with hashed passwords.
    /// WARNING: Only use in development environments!
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SeedDataController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SeedDataController> _logger;

        public SeedDataController(IConfiguration configuration, ILogger<SeedDataController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Seed test employees into the database.
        /// POST /api/seeddata/create-test-employees
        /// This creates three test accounts:
        /// - John Waiter (RoleID: 1 - Waiter)
        /// - Alice Kitchen (RoleID: 2 - Kitchen)
        /// - Bob Manager (RoleID: 3 - Manager)
        /// All with password: "test"
        /// </summary>
        [HttpPost("create-test-employees")]
        public IActionResult CreateTestEmployees()
        {
            try
            {
                var connStr = _configuration.GetConnectionString("ChapeauDatabaseSQL");
                if (string.IsNullOrEmpty(connStr))
                    return BadRequest(new { error = "Connection string not found" });

                var testEmployees = new[]
                {
                    new { Name = "John Waiter", Password = "test", RoleID = 1 },
                    new { Name = "Alice Kitchen", Password = "test", RoleID = 2 },
                    new { Name = "Bob Manager", Password = "test", RoleID = 3 }
                };

                var results = new List<object>();

                using (var connection = new SqlConnection(connStr))
                {
                    connection.Open();

                    foreach (var employee in testEmployees)
                    {
                        try
                        {
                            // Hash the password
                            string passwordHash = PasswordHasher.HashPassword(employee.Password);

                            // Check if employee already exists
                            using (var checkCommand = connection.CreateCommand())
                            {
                                checkCommand.CommandText = "SELECT COUNT(*) FROM Employee WHERE Name = @Name";
                                checkCommand.Parameters.AddWithValue("@Name", employee.Name);

                                int count = (int)checkCommand.ExecuteScalar();

                                if (count > 0)
                                {
                                    results.Add(new
                                    {
                                        name = employee.Name,
                                        status = "skipped",
                                        message = "Employee already exists"
                                    });
                                    continue;
                                }
                            }

                            // Insert the employee
                            using (var insertCommand = connection.CreateCommand())
                            {
                                insertCommand.CommandText = @"
                                    INSERT INTO Employee (Name, PasswordHash, RoleID, IsActive)
                                    VALUES (@Name, @PasswordHash, @RoleID, 1)";

                                insertCommand.Parameters.AddWithValue("@Name", employee.Name);
                                insertCommand.Parameters.AddWithValue("@PasswordHash", passwordHash);
                                insertCommand.Parameters.AddWithValue("@RoleID", employee.RoleID);

                                int rowsAffected = insertCommand.ExecuteNonQuery();

                                results.Add(new
                                {
                                    name = employee.Name,
                                    status = rowsAffected > 0 ? "created" : "failed",
                                    roleID = employee.RoleID,
                                    testPassword = employee.Password
                                });

                                _logger.LogInformation($"Created test employee: {employee.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            results.Add(new
                            {
                                name = employee.Name,
                                status = "error",
                                error = ex.Message
                            });
                            _logger.LogError($"Error creating employee {employee.Name}: {ex.Message}");
                        }
                    }
                }

                return Ok(new
                {
                    message = "✓ Test data seeding completed",
                    testAccounts = results,
                    instructions = new
                    {
                        loginUrl = "/Account/Login",
                        testCredentials = new
                        {
                            account1 = new { name = "John Waiter", password = "test", role = "Waiter (RoleID: 1)" },
                            account2 = new { name = "Alice Kitchen", password = "test", role = "Kitchen (RoleID: 2)" },
                            account3 = new { name = "Bob Manager", password = "test", role = "Manager (RoleID: 3)" }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Seed data error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Clear all test employees from the database.
        /// POST /api/seeddata/clear-test-employees
        /// </summary>
        [HttpPost("clear-test-employees")]
        public IActionResult ClearTestEmployees()
        {
            try
            {
                var connStr = _configuration.GetConnectionString("ChapeauDatabaseSQL");
                if (string.IsNullOrEmpty(connStr))
                    return BadRequest(new { error = "Connection string not found" });

                var testNames = new[] { "John Waiter", "Alice Kitchen", "Bob Manager" };

                using (var connection = new SqlConnection(connStr))
                {
                    connection.Open();

                    foreach (var name in testNames)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM Employee WHERE Name = @Name";
                            command.Parameters.AddWithValue("@Name", name);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                return Ok(new
                {
                    message = "✓ Test employees cleared",
                    clearedAccounts = testNames
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all employees in the database with password hash status.
        /// GET /api/seeddata/list-employees
        /// </summary>
        [HttpGet("list-employees")]
        public IActionResult ListEmployees()
        {
            try
            {
                var connStr = _configuration.GetConnectionString("ChapeauDatabaseSQL");
                if (string.IsNullOrEmpty(connStr))
                    return BadRequest(new { error = "Connection string not found" });

                var employees = new List<object>();

                using (var connection = new SqlConnection(connStr))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT EmployeeID, Name, RoleID, IsActive,
                                   CASE WHEN PasswordHash IS NULL THEN 'NULL' ELSE 'SET' END as PasswordStatus
                            FROM Employee
                            ORDER BY Name";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                employees.Add(new
                                {
                                    employeeID = (int)reader["EmployeeID"],
                                    name = (string)reader["Name"],
                                    roleID = (int)reader["RoleID"],
                                    isActive = (bool)reader["IsActive"],
                                    passwordStatus = (string)reader["PasswordStatus"]
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    totalEmployees = employees.Count,
                    employees = employees
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
