using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginDiagnosticsController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public LoginDiagnosticsController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        /// <summary>
        /// Test database connection
        /// GET /api/logindiagnostics/test-connection
        /// </summary>
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var connStr = _configuration.GetConnectionString("ChapeauDatabaseSQL");
                if (string.IsNullOrEmpty(connStr))
                    return BadRequest("Connection string not found");

                // Try a simple query
                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT 1";
                        await command.ExecuteScalarAsync();
                    }
                }

                return Ok(new { message = "✓ Database connection successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test authentication with specific credentials
        /// GET /api/logindiagnostics/test-auth?username=John%20Waiter&password=anypassword
        /// </summary>
        [HttpGet("test-auth")]
        public async Task<IActionResult> TestAuth(string username = "John Waiter", string password = "test")
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DIAG] Testing authentication for: {username}");

                var employee = await _authService.AuthenticateAsync(username, password);

                if (employee != null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "✓ Authentication successful",
                        employee = new
                        {
                            employeeID = employee.EmployeeID,
                            name = employee.Name,
                            username = employee.Username,
                            role = employee.Role.ToString(),
                            isActive = employee.IsActive
                        }
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = "✗ Authentication failed - employee not found or inactive"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// List all active employees in the database
        /// GET /api/logindiagnostics/list-employees
        /// </summary>
        [HttpGet("list-employees")]
        public IActionResult ListEmployees()
        {
            try
            {
                var connStr = _configuration.GetConnectionString("ChapeauDatabaseSQL");
                var employees = new List<object>();

                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT e.EmployeeID, e.Name, e.RoleID, e.IsActive
                            FROM Employees e
                            ORDER BY e.Name";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                employees.Add(new
                                {
                                    employeeID = (int)reader["EmployeeID"],
                                    name = (string)reader["Name"],
                                    roleID = (int)reader["RoleID"],
                                    isActive = (bool)reader["IsActive"]
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    message = $"Found {employees.Count} employee(s)",
                    employees = employees
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if Roles table is populated
        /// GET /api/logindiagnostics/check-roles
        /// </summary>
        [HttpGet("check-roles")]
        public IActionResult CheckRoles()
        {
            try
            {
                var connStr = _configuration.GetConnectionString("ChapeauDatabaseSQL");
                var roles = new List<object>();

                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT RoleID, RoleName FROM Roles ORDER BY RoleID";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                roles.Add(new
                                {
                                    roleID = (int)reader["RoleID"],
                                    roleName = (string)reader["RoleName"]
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    message = $"Found {roles.Count} role(s)",
                    roles = roles
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Comprehensive diagnostics
        /// GET /api/logindiagnostics/full-check
        /// </summary>
        [HttpGet("full-check")]
        public async Task<IActionResult> FullCheck()
        {
            var results = new Dictionary<string, object>();

            // Test connection
            try
            {
                var connStr = _configuration.GetConnectionString("ChapeauDatabaseSQL");
                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    await connection.OpenAsync();
                }
                results["Connection"] = "✓ OK";
            }
            catch (Exception ex)
            {
                results["Connection"] = $"✗ FAILED: {ex.Message}";
            }

            // Check Roles table
            try
            {
                var connStr = _configuration.GetConnectionString("ChapeauDatabaseSQL");
                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Roles";
                        var count = (int)command.ExecuteScalar();
                        results["Roles Table"] = $"✓ OK ({count} roles)";
                    }
                }
            }
            catch (Exception ex)
            {
                results["Roles Table"] = $"✗ FAILED: {ex.Message}";
            }

            // Check Employees table
            try
            {
                var connStr = _configuration.GetConnectionString("ChapeauDatabaseSQL");
                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Employees WHERE IsActive = 1";
                        var count = (int)command.ExecuteScalar();
                        results["Active Employees"] = $"✓ OK ({count} employees)";
                    }
                }
            }
            catch (Exception ex)
            {
                results["Active Employees"] = $"✗ FAILED: {ex.Message}";
            }

            // Test authentication
            try
            {
                var employee = await _authService.AuthenticateAsync("John Waiter", "any");
                results["Authentication Test"] = employee != null 
                    ? "✓ OK (John Waiter found)" 
                    : "✗ FAILED (John Waiter not found)";
            }
            catch (Exception ex)
            {
                results["Authentication Test"] = $"✗ FAILED: {ex.Message}";
            }

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                diagnostics = results
            });
        }
    }
}
