using System.Security.Cryptography;

namespace Chapeau.Database
{
    /// <summary>
    /// Password hash generator for test users.
    /// This generates hashes using the EXACT same algorithm as AuthService.
    /// To use: Create a test console project and reference this class, or copy the methods.
    /// </summary>
    public class PasswordHashGenerator
    {
        public static void GenerateTestUserHashes()
        {
            Console.WriteLine("=== Test User Password Hash Generator ===\n");
            Console.WriteLine("This generates hashes using PBKDF2-SHA256 with 10,000 iterations.\n");

            var testUsers = new[]
            {
                new { Name = "John Waiter", Password = "waiter123", RoleID = 1 },
                new { Name = "Jane Chef", Password = "chef123", RoleID = 2 },
                new { Name = "Bob Manager", Password = "manager123", RoleID = 3 }
            };

            Console.WriteLine("=== Copy this SQL into your database ===\n");
            Console.WriteLine("-- Ensure Roles are inserted first!");
            Console.WriteLine("-- DELETE FROM Employees; -- If you want to clear existing test data");
            Console.WriteLine();
            Console.WriteLine("INSERT INTO Employees (Name, PasswordHash, RoleID, IsActive)");
            Console.WriteLine("VALUES");

            for (int i = 0; i < testUsers.Length; i++)
            {
                var user = testUsers[i];
                var hash = HashPassword(user.Password);

                string comma = i < testUsers.Length - 1 ? "," : ";";
                Console.WriteLine($"('{user.Name}', '{hash}', {user.RoleID}, 1){comma}");
            }

            Console.WriteLine();
            Console.WriteLine("=== Test Credentials ===");
            Console.WriteLine("Use the employee NAME as the username in the login form:\n");

            foreach (var user in testUsers)
            {
                Console.WriteLine($"Username: {user.Name}");
                Console.WriteLine($"Password: {user.Password}");
                Console.WriteLine($"Role: {(user.RoleID == 1 ? "Waiter" : user.RoleID == 2 ? "Kitchen" : "Manager")}");
                Console.WriteLine();
            }

            // Test verification
            Console.WriteLine("=== Verification Test ===");
            var testHash = HashPassword("waiter123");
            bool verified = VerifyPassword("waiter123", testHash);
            Console.WriteLine($"Hash verification test: {(verified ? "✓ PASSED" : "✗ FAILED")}");
        }

        public static string HashPassword(string password)
        {
            // Using PBKDF2 for password hashing - EXACT same algorithm as AuthService
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

        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                // Using PBKDF2 for password verification - EXACT same algorithm as AuthService
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
    }
}
