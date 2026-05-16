using System.Collections.Generic;
using Chapeau.Models;
using Chapeau.Repositories.Employee;
using Chapeau.Utilities;

namespace Chapeau.Services
{
    public class EmployeeService(IEmployeeRepository employeeRepository)
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;

        public List<Employee> GetEmployees()
        {
            return _employeeRepository.GetEmployees();
        }

        public Employee? GetEmployeeById(int id)
        {
            return _employeeRepository.GetEmployeeById(id);
        }

        public void AddEmployee(Employee employee)
        {
            // Hash the password before storing
            if (!string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                employee.PasswordHash = PasswordHasher.HashPassword(employee.PasswordHash);
            }

            _employeeRepository.AddEmployee(employee);
        }

        public void UpdateEmployee(Employee employee)
        {
            if (!string.IsNullOrWhiteSpace(employee.PasswordHash) && !IsAlreadyHashed(employee.PasswordHash))
            {
                employee.PasswordHash = PasswordHasher.HashPassword(employee.PasswordHash);
            }

            _employeeRepository.UpdateEmployee(employee);
        }

        public void DeleteEmployee(int id)
        {
            _employeeRepository.DeleteEmployee(id);
        }

        public void SetEmployeeActive(int id, bool active)
        {
            _employeeRepository.SetEmployeeActive(id, active);
        }

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
