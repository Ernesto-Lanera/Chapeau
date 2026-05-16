using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Chapeau.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseDiagnosticsController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public DatabaseDiagnosticsController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        /// <summary>
        /// Get all employees from the database to verify credentials exist
        /// GET /api/databasediagnostics/employees
        /// </summary>
        [HttpGet("employees")]
        public IActionResult GetAllEmployees()
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
                            SELECT e.EmployeeID, e.Name, e.Username, e.RoleID, e.IsActive, 
                                   CASE WHEN e.PasswordHash IS NULL THEN 'NULL' ELSE 'SET' END as PasswordHashStatus
                            FROM Employee e
                            ORDER BY e.Name";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                employees.Add(new
                                {
                                    employeeID = (int)reader["EmployeeID"],
                                    name = (string)reader["Name"],
                                    username = reader["Username"] == DBNull.Value ? null : (string)reader["Username"],
                                    roleID = (int)reader["RoleID"],
                                    isActive = (bool)reader["IsActive"],
                                    passwordHashStatus = (string)reader["PasswordHashStatus"]
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    totalEmployees = employees.Count,
                    activeEmployees = employees.OfType<dynamic>().Count(e => e.isActive),
                    employees = employees
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test login with specific credentials
        /// GET /api/databasediagnostics/test-login?username=John%20Waiter&password=test
        /// </summary>
        [HttpGet("test-login")]
        public async Task<IActionResult> TestLogin(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return BadRequest(new { error = "Username and password are required" });
                }

                var employee = await _authService.AuthenticateAsync(username, password);

                if (employee != null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "✓ Login successful",
                        employee = new
                        {
                            employeeID = employee.EmployeeID,
                            name = employee.Name,
                            roleID = employee.RoleID,
                            isActive = employee.IsActive
                        }
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = "✗ Login failed - credentials invalid or user inactive"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Check if a specific employee exists in the database
        /// GET /api/databasediagnostics/check-employee?name=John%20Waiter
        /// </summary>
        [HttpGet("check-employee")]
        public IActionResult CheckEmployee(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return BadRequest(new { error = "Name is required" });

                var connStr = _configuration.GetConnectionString("ChapeauDatabaseSQL");

                using (var connection = new SqlConnection(connStr))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT e.EmployeeID, e.Name, e.RoleID, e.IsActive,
                                   CASE WHEN e.PasswordHash IS NULL THEN 'NULL' ELSE 'SET' END as PasswordHashStatus
                            FROM Employee e
                            WHERE e.Name = @Name";

                        command.Parameters.AddWithValue("@Name", name);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new
                                {
                                    found = true,
                                    message = "✓ Employee found",
                                    employee = new
                                    {
                                        employeeID = (int)reader["EmployeeID"],
                                        name = (string)reader["Name"],
                                        roleID = (int)reader["RoleID"],
                                        isActive = (bool)reader["IsActive"],
                                        passwordHashSet = (string)reader["PasswordHashStatus"] == "SET"
                                    }
                                });
                            }
                            else
                            {
                                return Ok(new
                                {
                                    found = false,
                                    message = $"✗ Employee '{name}' not found in database"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
