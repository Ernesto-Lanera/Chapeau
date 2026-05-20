using Chapeau.Repositories;
using Chapeau.Utilities;
using Microsoft.Extensions.Logging;
using EmployeeModel = Chapeau.Models.Employee;

namespace Chapeau.Services.Overview
{
    public class EmployeeService(EmployeeRepository employeeRepository, ILogger<EmployeeService> logger)
    {
        private readonly EmployeeRepository _employeeRepository = employeeRepository;
        private readonly ILogger<EmployeeService> _logger = logger;

        // Haalt alle medewerkers uit de database
        public List<EmployeeModel> GetEmployees()
        {
            return _employeeRepository.GetEmployees();
        }

        // Zoekt medewerker op naam
        public EmployeeModel? GetEmployeeByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return _employeeRepository.GetEmployeeByName(name);
        }

        public void AddEmployee(EmployeeModel employee)
        {
            ArgumentNullException.ThrowIfNull(employee);

            if (string.IsNullOrWhiteSpace(employee.Name))
                throw new ArgumentException("Naam is verplicht.");

            if (employee.RoleID <= 0)
                throw new ArgumentException("Kies een geldige rol.");

            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
                throw new ArgumentException("Wachtwoord is verplicht.");

            employee.PasswordHash = PasswordHasher.HashPassword(employee.PasswordHash);
            employee.IsActive = true;

            _employeeRepository.AddEmployee(employee);
            _logger.LogInformation("Employee created: {EmployeeName} (Role: {RoleID})", employee.Name, employee.RoleID);
        }

        // Werkt bestaande medewerker bij, voorkomt wachtwoord overwrite als niet gewijzigd
        public void UpdateEmployee(EmployeeModel employee)
        {
            ArgumentNullException.ThrowIfNull(employee);

            if (employee.EmployeeID <= 0)
                throw new ArgumentException("Ongeldige medewerker.");

            if (string.IsNullOrWhiteSpace(employee.Name))
                throw new ArgumentException("Naam is verplicht.");

            if (employee.RoleID <= 0)
                throw new ArgumentException("Kies een geldige rol.");

            var existingEmployee = _employeeRepository.GetEmployeeById(employee.EmployeeID)
                ?? throw new ArgumentException("Medewerker bestaat niet.");

            // Behoud bestaand wachtwoord als niets ingevuld
            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                employee.PasswordHash = existingEmployee.PasswordHash;
            }
            else if (!IsAlreadyHashed(employee.PasswordHash))
            {
                // Hash alleen als het nog niet gehashed is
                employee.PasswordHash = PasswordHasher.HashPassword(employee.PasswordHash);
            }

            _employeeRepository.UpdateEmployee(employee);
            _logger.LogInformation("Employee updated: {EmployeeID} - {EmployeeName}", employee.EmployeeID, employee.Name);
        }

        // Zet medewerker actief of inactief
        public void SetEmployeeActive(int id, bool active)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Ongeldige medewerker.");
            }

            _employeeRepository.SetEmployeeActive(id, active);
        }

        // Test database connectie
        public bool TestConnection()
        {
            return _employeeRepository.TestConnection();
        }

        // Check of wachtwoord al gehashed is (64 chars base64)
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
