using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.Utilities;
using Chapeau.ViewModels;

namespace Chapeau.Services.Overview
{
    public class EmployeeService(
        IEmployeeRepository employeeRepository,
        IRoleRepository roleRepository,
        ILogger<EmployeeService> logger) : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IRoleRepository _roleRepository = roleRepository;
        private readonly ILogger<EmployeeService> _logger = logger;

        public EmployeeManagementViewModel GetManagementOverview(int? editId, bool showCreate)
        {
            IReadOnlyList<Employee> employees = _employeeRepository.GetEmployees();
            return new EmployeeManagementViewModel
            {
                Employees = employees,
                Roles = _roleRepository.GetRoles(),
                EditEmployee = editId.HasValue
                    ? employees.FirstOrDefault(employee => employee.EmployeeID == editId.Value)
                    : null,
                ShowCreate = showCreate
            };
        }

        public List<Employee> GetEmployees() => _employeeRepository.GetEmployees();

        public void AddEmployee(EmployeeInputModel input)
        {
            ValidateInput(input, requirePassword: true, excludedEmployeeId: null);
            var employee = new Employee
            {
                Name = input.Name.Trim(),
                PasswordHash = PasswordHasher.HashPassword(input.Password),
                IsActive = true
            };
            employee.AssignRole(GetRole(input.RoleID));

            _employeeRepository.AddEmployee(employee);
            _logger.LogInformation("Medewerker aangemaakt: {EmployeeId}.", employee.EmployeeID);
        }

        public void UpdateEmployee(EmployeeInputModel input)
        {
            if (input.EmployeeID <= 0)
            {
                throw new ArgumentException("Ongeldig medewerker ID.", nameof(input.EmployeeID));
            }

            Employee employee = _employeeRepository.GetEmployeeById(input.EmployeeID)
                ?? throw new InvalidOperationException(ErrorMessages.EmployeeNotFound);

            ValidateInput(input, requirePassword: false, excludedEmployeeId: employee.EmployeeID);
            employee.Name = input.Name.Trim();
            employee.AssignRole(GetRole(input.RoleID));

            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                employee.PasswordHash = PasswordHasher.HashPassword(input.Password);
            }

            _employeeRepository.UpdateEmployee(employee);
            _logger.LogInformation("Medewerker gewijzigd: {EmployeeId}.", employee.EmployeeID);
        }

        public void SetEmployeeActive(int employeeId, bool active)
        {
            if (employeeId <= 0)
            {
                throw new ArgumentException("Ongeldig medewerker ID.", nameof(employeeId));
            }

            _employeeRepository.SetEmployeeActive(employeeId, active);
            _logger.LogInformation("Medewerkerstatus gewijzigd: {EmployeeId} - {Active}.", employeeId, active);
        }

        public bool TestConnection() => _employeeRepository.TestConnection();

        private void ValidateInput(EmployeeInputModel input, bool requirePassword, int? excludedEmployeeId)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new ArgumentException(ErrorMessages.EmployeeNameRequired);
            }

            if (input.Name.Trim().Length > 100)
            {
                throw new ArgumentException(ErrorMessages.EmployeeNameTooLong);
            }

            if (requirePassword && string.IsNullOrWhiteSpace(input.Password))
            {
                throw new ArgumentException(ErrorMessages.EmployeePasswordRequired);
            }

            if (_employeeRepository.NameExists(input.Name.Trim(), excludedEmployeeId))
            {
                throw new InvalidOperationException(ErrorMessages.UsernameTaken);
            }

            GetRole(input.RoleID);
        }

        private EmployeeRole GetRole(int roleId) =>
            _roleRepository.GetRoles().FirstOrDefault(role => role.RoleID == roleId)
            ?? throw new ArgumentException(ErrorMessages.EmployeeRoleRequired);
    }
}
