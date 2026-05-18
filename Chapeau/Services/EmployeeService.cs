using System.Collections.Generic;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.Utilities;

namespace Chapeau.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepository _employeeRepository;

        public EmployeeService(EmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public List<Employee> GetEmployees()
        {
            return _employeeRepository.GetEmployees();
        }

        public Employee? GetEmployeeByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return _employeeRepository.GetEmployeeByName(name);
        }

        public Employee? ValidateLogin(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            Employee? employee = _employeeRepository.GetEmployeeByName(name);

            if (employee == null)
            {
                return null;
            }

            if (!employee.IsActive)
            {
                return null;
            }

            bool passwordIsValid = PasswordHasher.VerifyPassword(password, employee.PasswordHash);

            if (!passwordIsValid)
            {
                return null;
            }

            return employee;
        }

        public void AddEmployee(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                throw new ArgumentException("Naam is verplicht.");
            }

            if (employee.RoleID <= 0)
            {
                throw new ArgumentException("Kies een geldige rol.");
            }

            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                throw new ArgumentException("Wachtwoord is verplicht.");
            }

            employee.PasswordHash = PasswordHasher.HashPassword(employee.PasswordHash);
            employee.IsActive = true;

            _employeeRepository.AddEmployee(employee);
        }

        public void UpdateEmployee(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            if (employee.EmployeeID <= 0)
            {
                throw new ArgumentException("Ongeldige medewerker.");
            }

            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                throw new ArgumentException("Naam is verplicht.");
            }

            if (employee.RoleID <= 0)
            {
                throw new ArgumentException("Kies een geldige rol.");
            }

            Employee? existingEmployee = _employeeRepository.GetEmployeeById(employee.EmployeeID);

            if (existingEmployee == null)
            {
                throw new ArgumentException("Medewerker bestaat niet.");
            }

            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                employee.PasswordHash = existingEmployee.PasswordHash;
            }
            else if (!IsAlreadyHashed(employee.PasswordHash))
            {
                employee.PasswordHash = PasswordHasher.HashPassword(employee.PasswordHash);
            }

            _employeeRepository.UpdateEmployee(employee);
        }

        public void SetEmployeeActive(int id, bool active)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Ongeldige medewerker.");
            }

            _employeeRepository.SetEmployeeActive(id, active);
        }

        private static bool IsAlreadyHashed(string password)
        {
            return password.Length == 64 && IsValidBase64(password);
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