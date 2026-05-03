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
                        SELECT EmployeeID, Name, Username, PasswordHash, Role, IsActive
                        FROM Employee
                        WHERE Username = @Username AND IsActive = 1";

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
                                    Username = (string)reader["Username"],
                                    PasswordHash = (string)reader["PasswordHash"],
                                    Role = (string)reader["Role"],
                                    IsActive = (bool)reader["IsActive"]
                                };

                                if (VerifyPassword(password, employee.PasswordHash))
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
                System.Diagnostics.Debug.WriteLine($"Authentication error: {ex.Message}");
            }

            return null;
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
                        return false;
                }

                return true;
            }
            catch
            {
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
