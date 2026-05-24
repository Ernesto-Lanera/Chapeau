using Chapeau.Models;

namespace Chapeau.Repositories
{
    public interface IEmployeeRepository
    {
        List<Employee> GetEmployees();
        Employee? GetEmployeeById(int employeeId);
        Employee? GetEmployeeByName(string name);
        Task<Employee?> GetEmployeeByNameAsync(string name);
        bool NameExists(string name, int? excludedEmployeeId = null);
        void AddEmployee(Employee employee);
        void UpdateEmployee(Employee employee);
        void SetEmployeeActive(int employeeId, bool active);
        bool TestConnection();
    }
}
