using Chapeau.Models;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Chapeau.Services
{
    public interface IAuthService
    {
        Task<Employee?> AuthenticateAsync(string username, string password);
        bool VerifyPassword(string password, string hash);
        string HashPassword(string password);
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
                        SELECT e.EmployeeID, e.Name, e.PasswordHash, e.RoleID, e.IsActive
                        FROM Employee e
                        WHERE e.Name = @Username AND e.IsActive = 1";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var employeeID = (int)reader["EmployeeID"];
                                var employeeName = (string)reader["Name"];
                                var passwordHash = (string)reader["PasswordHash"];
                                var roleId = (int)reader["RoleID"];
                                var isActive = (bool)reader["IsActive"];

                                // Verify password
                                bool passwordValid = VerifyPassword(password, passwordHash);

                                if (passwordValid)
                                {
                                    // Map RoleID directly to EmployeeRole enum without database lookup
                                    var role = MapRoleIdToRole(roleId);

                                    var employee = new Employee
                                    {
                                        EmployeeID = employeeID,
                                        Name = employeeName,
                                        Username = employeeName,
                                        PasswordHash = passwordHash,
                                        Role = role,
                                        IsActive = isActive
                                    };

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

        /// <summary>
        /// Maps database RoleID directly to EmployeeRole enum.
        /// Avoids extra database lookup during authentication.
        /// </summary>
        private EmployeeRole MapRoleIdToRole(int roleId)
        {
            return roleId switch
            {
                1 => EmployeeRole.Waiter,
                2 => EmployeeRole.Kitchen,
                3 => EmployeeRole.Manager,
                _ => EmployeeRole.Waiter  // Default to Waiter
            };
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                // Using PBKDF2 for password verification
                var hashBytes = Convert.FromBase64String(hash);
                var salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);

                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                byte[] hash2 = pbkdf2.GetBytes(20);

                for (int i = 0; i < 20; i++)
                {
                    if (hashBytes[i + 16] != hash2[i])
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VERIFY] Error during password verification: {ex.Message}");
                return false;
            }
        }

        public string HashPassword(string password)
        {
            // Using PBKDF2 for password hashing
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
