using Chapeau.Models;

namespace Chapeau.Services.Login
{
    public enum AuthenticationStatus
    {
        Success,
        InvalidCredentials,
        InactiveAccount
    }

    public sealed class AuthenticationResult
    {
        private AuthenticationResult(AuthenticationStatus status, Employee? employee = null)
        {
            Status = status;
            Employee = employee;
        }

        public AuthenticationStatus Status { get; }
        public Employee? Employee { get; }

        public static AuthenticationResult Success(Employee employee) =>
            new(AuthenticationStatus.Success, employee);

        public static AuthenticationResult InvalidCredentials() =>
            new(AuthenticationStatus.InvalidCredentials);

        public static AuthenticationResult InactiveAccount() =>
            new(AuthenticationStatus.InactiveAccount);
    }
}
