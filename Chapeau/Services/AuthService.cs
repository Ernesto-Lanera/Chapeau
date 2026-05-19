using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.Utilities;

namespace Chapeau.Services
{
    public interface IAuthService
    {
        Task<Employee?> AuthenticateAsync(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly EmployeeRepository _employeeRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(EmployeeRepository employeeRepository, ILogger<AuthService> logger)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<Employee?> AuthenticateAsync(string username, string password)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeeByNameAsync(username);

                if (employee != null && PasswordHasher.VerifyPassword(password, employee.PasswordHash))
                {
                    return employee;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for user {Username}", username);
            }

            return null;
        }
    }
}
