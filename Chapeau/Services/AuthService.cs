using Chapeau.Models;
using Chapeau.Utilities;
using Microsoft.Data.SqlClient;

namespace Chapeau.Services
{
    public interface IAuthService
    {
        Task<Employee?> AuthenticateAsync(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Employee?> AuthenticateAsync(string username, string password)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("ChapeauDatabaseSQL")
                    ?? throw new Exception("Database connection string is missing.");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT EmployeeID, Name, PasswordHash, RoleID, IsActive
                        FROM Employee
                        WHERE Name = @Username AND IsActive = 1";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var employee = new Employee
                                {
                                    EmployeeID = (int)reader["EmployeeID"],
                                    Name = (string)reader["Name"],
                                    PasswordHash = (string)reader["PasswordHash"],
                                    RoleID = (int)reader["RoleID"],
                                    IsActive = (bool)reader["IsActive"]
                                };

                                // Verify password using PasswordHasher utility
                                if (PasswordHasher.VerifyPassword(password, employee.PasswordHash))
                                {
                                    return employee;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH] Authentication error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[AUTH] Stack trace: {ex.StackTrace}");
            }

            return null;
        }
    }
}