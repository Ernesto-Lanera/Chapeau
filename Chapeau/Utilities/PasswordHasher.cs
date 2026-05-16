using System;
using System.Security.Cryptography;
using System.Text;

namespace Chapeau.Utilities
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        /// <summary>
        /// Hashes a password using PBKDF2 with SHA-256.
        /// Returns a string containing the salt and hash in Base64 format.
        /// </summary>
        public static string HashPassword(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(HashSize);

                    // Combine salt and hash: salt + hash
                    byte[] combined = new byte[SaltSize + HashSize];
                    Array.Copy(salt, 0, combined, 0, SaltSize);
                    Array.Copy(hash, 0, combined, SaltSize, HashSize);

                    // Return as Base64 string
                    return Convert.ToBase64String(combined);
                }
            }
        }

        /// <summary>
        /// Verifies a password against a stored hash.
        /// Returns true if the password matches the hash.
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(hash);

            try
            {
                // Decode the hash from Base64
                byte[] combined = Convert.FromBase64String(hash);

                // Extract salt and stored hash
                byte[] salt = new byte[SaltSize];
                byte[] storedHash = new byte[HashSize];

                Array.Copy(combined, 0, salt, 0, SaltSize);
                Array.Copy(combined, SaltSize, storedHash, 0, HashSize);

                // Compute hash of the provided password
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(HashSize);

                    // Compare hashes in constant time
                    return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}