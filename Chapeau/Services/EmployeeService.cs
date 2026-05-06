using System.Collections.Generic;
using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.Utilities;

namespace Chapeau.Services
{
    /// Service for employee-related business logic.
    public class EmployeeService(EmployeeRepository employeeRepository, ILogger<EmployeeService> logger)
    {
        private readonly EmployeeRepository _employeeRepository = employeeRepository;
        private readonly ILogger<EmployeeService> _logger = logger;

        public List<Employee> GetEmployees()
        {
            return _employeeRepository.GetEmployees();
        }

        /// Gets only active employees.
        public List<Employee> GetActiveEmployees()
        {
            var employees = _employeeRepository.GetEmployees();
            return employees.Where(x => x.IsActive).ToList();
        }

        /// Gets employees by role.
        public List<Employee> GetEmployeesByRole(EmployeeRole role)
        {
            var employees = _employeeRepository.GetEmployees();
            return employees.Where(x => x.Role == role).ToList();
        }

        /// Adds an employee with validation.
        public void AddEmployee(Employee employee)
        {
            // Hash the password before storing
            if (!string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                employee.PasswordHash = PasswordHasher.HashPassword(employee.PasswordHash);
            }

            _employeeRepository.AddEmployee(employee);
            _logger.LogInformation("Employee added successfully: {EmployeeName}", employee.Name);
        }

        /// Updates an employee with validation.
        public void UpdateEmployee(Employee employee)
        {
            // Hash the password only if it's a new plaintext password (not already hashed)
            if (!string.IsNullOrWhiteSpace(employee.PasswordHash) && !IsAlreadyHashed(employee.PasswordHash))
            {
                employee.PasswordHash = PasswordHasher.HashPassword(employee.PasswordHash);
            }

            _employeeRepository.UpdateEmployee(employee);
            _logger.LogInformation("Employee updated successfully: {EmployeeId}", employee.EmployeeID);
        }

        public void SetEmployeeActive(int id, bool active)
        {
            _employeeRepository.SetEmployeeActive(id, active);
            _logger.LogInformation("Employee {EmployeeId} active status: {IsActive}", id, active);
        }

        private void ValidateEmployee(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Name))
                throw new ArgumentException("Employee name is required");

            if (string.IsNullOrWhiteSpace(employee.Username))
                throw new ArgumentException("Employee username is required");

            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
                throw new ArgumentException("Employee password is required");

            if (employee.Name.Length > DatabaseConstraints.EmployeeNameMaxLength)
                throw new ArgumentException($"Employee name cannot exceed {DatabaseConstraints.EmployeeNameMaxLength} characters");

            if (employee.Username.Length > DatabaseConstraints.EmployeeUsernameMaxLength)
                throw new ArgumentException($"Employee username cannot exceed {DatabaseConstraints.EmployeeUsernameMaxLength} characters");
        }

        /// <summary>
        /// Simple heuristic to check if a password is already hashed.
        /// Hashed passwords are Base64 strings with 64+ characters.
        /// </summary>
        private static bool IsAlreadyHashed(string password)
        {
            return password.Length > 60 && IsValidBase64(password);
        }

        private static bool IsValidBase64(string input)
        {
            try
            {
                Convert.FromBase64String(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
