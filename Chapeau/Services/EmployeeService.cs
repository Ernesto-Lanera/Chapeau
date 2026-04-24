using System.Collections.Generic;
using Chapeau.Models;
using Chapeau.Repositories;

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

        public void AddEmployee(Employee employee)
        {
            _employeeRepository.AddEmployee(employee);
        }

        public void UpdateEmployee(Employee employee)
        {
            _employeeRepository.UpdateEmployee(employee);
        }

        public void SetEmployeeActive(int id, bool active)
        {
            _employeeRepository.SetEmployeeActive(id, active);
        }
    }
}
