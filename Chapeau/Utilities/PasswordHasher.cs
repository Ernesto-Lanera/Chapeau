using System;
using System.Security.Cryptography;
using System.Text;

namespace Chapeau.Utilities
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;
        public static string HashPassword(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            // Generate a random salt
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);

                // Hash the password using PBKDF2
                var hash = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: salt,
                    iterations: Iterations,
                    hashAlgorithm: HashAlgorithmName.SHA256,
                    outputLength: HashSize);

                // Combine salt and hash
                byte[] hashWithSalt = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashWithSalt, 0, SaltSize);
                Array.Copy(hash, 0, hashWithSalt, SaltSize, HashSize);

                // Return as Base64 string
                return Convert.ToBase64String(hashWithSalt);
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(hash);

            try
            {
                // Convert the hash from Base64 string
                byte[] hashWithSalt = Convert.FromBase64String(hash);

                // Extract salt and hash
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashWithSalt, 0, salt, 0, SaltSize);

                // Hash the provided password with the extracted salt
                var hashOfInput = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: salt,
                    iterations: Iterations,
                    hashAlgorithm: HashAlgorithmName.SHA256,
                    outputLength: HashSize);

                // Compare hashes
                return CryptographicOperations.FixedTimeEquals(
                    hashOfInput, 
                    new ReadOnlySpan<byte>(hashWithSalt, SaltSize, HashSize));
            }
            catch
            {
                return false;
            }
        }
    }
}
