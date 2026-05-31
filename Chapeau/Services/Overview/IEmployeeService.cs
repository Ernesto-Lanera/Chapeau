using Chapeau.Models;
using Chapeau.ViewModels;

namespace Chapeau.Services.Overview
{
    public interface IEmployeeService
    {
        EmployeeManagementViewModel GetManagementOverview(int? editId, bool showCreate);
        List<Employee> GetEmployees();
        void AddEmployee(EmployeeInputModel input);
        void UpdateEmployee(EmployeeInputModel input);
        void SetEmployeeActive(int employeeId, bool active);
        bool TestConnection();
    }
}
