using Chapeau.Models;

namespace Chapeau.Services.Login
{
    /// <summary>
    /// Represents the possible outcomes of an authentication attempt.
    /// </summary>
    public enum AuthenticationStatus
    {
        /// <summary>Authentication succeeded.</summary>
        Success,
        /// <summary>The provided name or password does not match any active record.</summary>
        InvalidCredentials,
        /// <summary>The employee exists but their account has been deactivated.</summary>
        InactiveAccount
    }

    /// <summary>
    /// Encapsulates the result of an authentication attempt including status and employee data.
    /// </summary>
    public sealed class AuthenticationResult
    {
        private AuthenticationResult(AuthenticationStatus status, Employee? employee = null)
        {
            Status = status;
            Employee = employee;
        }

        /// <summary>Gets the authentication outcome status.</summary>
        public AuthenticationStatus Status { get; }
        /// <summary>Gets the authenticated employee, or null if authentication failed.</summary>
        public Employee? Employee { get; }

        /// <summary>Creates a successful authentication result with the authenticated employee.</summary>
        public static AuthenticationResult Success(Employee employee) =>
            new(AuthenticationStatus.Success, employee);

        /// <summary>Creates a failed authentication result for invalid credentials.</summary>
        public static AuthenticationResult InvalidCredentials() =>
            new(AuthenticationStatus.InvalidCredentials);

        /// <summary>Creates a failed authentication result for a deactivated account.</summary>
        public static AuthenticationResult InactiveAccount() =>
            new(AuthenticationStatus.InactiveAccount);
    }
}
