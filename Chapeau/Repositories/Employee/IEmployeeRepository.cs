using Chapeau.Models;

namespace Chapeau.Repositories
{
    /// <summary>
    /// Repository interface for employee data access.
    /// </summary>
    public interface IEmployeeRepository
    {
        /// <summary>Gets all employees ordered by name.</summary>
        List<Employee> GetEmployees();
        /// <summary>Gets a single employee by ID, or null if not found.</summary>
        Employee? GetEmployeeById(int employeeId);
        /// <summary>Gets a single employee by name, or null if not found.</summary>
        Employee? GetEmployeeByName(string name);
        /// <summary>Async version of GetEmployeeByName.</summary>
        Task<Employee?> GetEmployeeByNameAsync(string name);
        /// <summary>Checks if an employee name already exists, optionally excluding a specific employee ID.</summary>
        bool NameExists(string name, int? excludedEmployeeId = null);
        /// <summary>Adds a new employee to the database.</summary>
        void AddEmployee(Employee employee);
        /// <summary>Updates an existing employee's details.</summary>
        void UpdateEmployee(Employee employee);
        /// <summary>Sets the active/inactive status of an employee.</summary>
        void SetEmployeeActive(int employeeId, bool active);
        /// <summary>Tests whether the database connection is available.</summary>
        bool TestConnection();
    }
}
