using System.Collections.Generic;
using Chapeau.Models;

namespace Chapeau.Repositories.Employee
{
    public interface IEmployeeRepository
    {
        List<Chapeau.Models.Employee> GetEmployees();
        Chapeau.Models.Employee? GetEmployeeById(int id);
        void AddEmployee(Chapeau.Models.Employee employee);
        void UpdateEmployee(Chapeau.Models.Employee employee);
        void SetEmployeeActive(int id, bool active);
    }
}