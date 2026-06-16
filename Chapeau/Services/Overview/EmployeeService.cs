using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.Utilities;
using Chapeau.ViewModels;

namespace Chapeau.Services.Overview
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IRoleRepository roleRepository,
            ILogger<EmployeeService> logger)
        {
            _employeeRepository = employeeRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public EmployeeManagementViewModel GetManagementOverview(int? editId, bool showCreate)
        {
            List<Employee> employees = _employeeRepository.GetEmployees();

            Employee? editEmployee = null;
            if (editId.HasValue)
            {
                editEmployee = employees.FirstOrDefault(employee => employee.EmployeeID == editId.Value);
            }

            return new EmployeeManagementViewModel
            {
                Employees = employees,
                Roles = _roleRepository.GetRoles(),
                EditEmployee = editEmployee,
                ShowCreate = showCreate
            };
        }

        public List<Employee> GetEmployees()
        {
            return _employeeRepository.GetEmployees();
        }

        public void AddEmployee(EmployeeInputModel input)
        {
            // Bij een nieuwe medewerker is een wachtwoord verplicht.
            ValidateInput(input, true, null);

            Employee employee = new Employee
            {
                Name = input.Name,
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

            Employee? employee = _employeeRepository.GetEmployeeById(input.EmployeeID);
            if (employee == null)
            {
                throw new InvalidOperationException(ErrorMessages.EmployeeNotFound);
            }

            // Bij wijzigen mag het wachtwoord leeg blijven.
            ValidateInput(input, false, employee.EmployeeID);
            employee.Name = input.Name;
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

        public bool TestConnection()
        {
            return _employeeRepository.TestConnection();
        }

        private void ValidateInput(EmployeeInputModel input, bool requirePassword, int? excludedEmployeeId)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new ArgumentException(ErrorMessages.EmployeeNameRequired);
            }

            if (input.Name.Length > 100)
            {
                throw new ArgumentException(ErrorMessages.EmployeeNameTooLong);
            }

            if (requirePassword && string.IsNullOrWhiteSpace(input.Password))
            {
                throw new ArgumentException(ErrorMessages.EmployeePasswordRequired);
            }

            if (_employeeRepository.NameExists(input.Name, excludedEmployeeId))
            {
                throw new InvalidOperationException(ErrorMessages.UsernameTaken);
            }

            GetRole(input.RoleID);
        }

        private EmployeeRole GetRole(int roleId)
        {
            EmployeeRole? role = _roleRepository.GetRoles().FirstOrDefault(item => item.RoleID == roleId);
            if (role == null)
            {
                throw new ArgumentException(ErrorMessages.EmployeeRoleRequired);
            }

            return role;
        }
    }
}
