using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.Utilities;

namespace Chapeau.Services.Login
{
    /// <summary>
    /// Service for authenticating users by credentials and validating their status.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticate a user by login identifier (username/employee number) and password.
        /// Returns success status with employee info, or error status if authentication fails.
        /// </summary>
        Task<AuthenticationResult> AuthenticateAsync(string loginIdentifier, string password);
    }

    /// <summary>
    /// Implementation of user authentication with password hashing and status checks.
    /// </summary>
    public class AuthService(IEmployeeRepository employeeRepository, ILogger<AuthService> logger) : IAuthService
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly ILogger<AuthService> _logger = logger;

        public async Task<AuthenticationResult> AuthenticateAsync(string loginIdentifier, string password)
        {
            if (string.IsNullOrWhiteSpace(loginIdentifier) || string.IsNullOrWhiteSpace(password))
            {
                return AuthenticationResult.InvalidCredentials();
            }

            try
            {
                Employee? employee = await _employeeRepository.GetEmployeeByNameAsync(loginIdentifier);

                if (employee is null || !PasswordHasher.VerifyPassword(password, employee.PasswordHash))
                {
                    return AuthenticationResult.InvalidCredentials();
                }

                if (!employee.IsActive)
                {
                    return AuthenticationResult.InactiveAccount();
                }

                return AuthenticationResult.Success(employee);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Aanmelden mislukt voor medewerker {LoginIdentifier}.", loginIdentifier);
                return AuthenticationResult.InvalidCredentials();
            }
        }
    }
}
