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

        public AuthService(EmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
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
                System.Diagnostics.Debug.WriteLine($"[AUTH] Error: {ex.Message}");
            }

            return null;
        }
    }
}