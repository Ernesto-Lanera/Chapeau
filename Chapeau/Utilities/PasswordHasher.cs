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

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);

                var hash = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: salt,
                    iterations: Iterations,
                    hashAlgorithm: HashAlgorithmName.SHA256,
                    outputLength: HashSize);

                byte[] hashWithSalt = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashWithSalt, 0, SaltSize);
                Array.Copy(hash, 0, hashWithSalt, SaltSize, HashSize);

                return Convert.ToBase64String(hashWithSalt);
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(hash);

            try
            {
                byte[] hashWithSalt = Convert.FromBase64String(hash);

                byte[] salt = new byte[SaltSize];
                Array.Copy(hashWithSalt, 0, salt, 0, SaltSize);

                var hashOfInput = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: salt,
                    iterations: Iterations,
                    hashAlgorithm: HashAlgorithmName.SHA256,
                    outputLength: HashSize);

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
