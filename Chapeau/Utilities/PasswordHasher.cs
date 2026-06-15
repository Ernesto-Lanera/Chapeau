using System;
using System.Security.Cryptography;
using System.Text;

namespace Chapeau.Utilities
{
    /// <summary>
    /// Provides secure password hashing using PBKDF2-SHA256 with a 100,000 iteration work factor.
    /// Uses constant-time comparison to prevent timing attacks.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        /// <summary>Hashes a password using PBKDF2-SHA256 with a random 16-byte salt.</summary>
        public static string HashPassword(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            byte[] salt = GenerateSalt();
            byte[] hash = ComputeHash(password, salt);
            byte[] combined = CombineSaltAndHash(salt, hash);

            return Convert.ToBase64String(combined);
        }

        /// <summary>
        /// Verifies a password against a stored hash using constant-time comparison.
        /// Returns false if the hash format is invalid or the password does not match.
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(hash);

            try
            {
                byte[] combined = Convert.FromBase64String(hash);
                byte[] salt = ExtractSalt(combined);
                byte[] storedHash = ExtractHash(combined);
                byte[] computedHash = ComputeHash(password, salt);

                return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Generates a cryptographically random salt using a secure RNG.</summary>
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>Computes the PBKDF2-SHA256 hash of the password with the given salt.</summary>
        private static byte[] ComputeHash(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }

        /// <summary>Combines salt and hash into a single byte array for storage.</summary>
        private static byte[] CombineSaltAndHash(byte[] salt, byte[] hash)
        {
            byte[] combined = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, combined, 0, SaltSize);
            Array.Copy(hash, 0, combined, SaltSize, HashSize);
            return combined;
        }

        /// <summary>Extracts the salt from the beginning of a combined salt+hash array.</summary>
        private static byte[] ExtractSalt(byte[] combined)
        {
            byte[] salt = new byte[SaltSize];
            Array.Copy(combined, 0, salt, 0, SaltSize);
            return salt;
        }

        /// <summary>Extracts the hash from the end of a combined salt+hash array.</summary>
        private static byte[] ExtractHash(byte[] combined)
        {
            byte[] hash = new byte[HashSize];
            Array.Copy(combined, SaltSize, hash, 0, HashSize);
            return hash;
        }
    }
}