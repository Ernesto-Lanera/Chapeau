using Microsoft.AspNetCore.Mvc;
using Chapeau.Models;

namespace Chapeau.Components.EmployeeTable;

public class EmployeeTableViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        IEnumerable<Employee> employees,
        IEnumerable<EmployeeRole> roles,
        Employee? editEmployee)
    {
        var model = new EmployeeTableModel
        {
            Employees = employees ?? [],
            RoleNameById = (roles ?? []).ToDictionary(r => r.RoleID, r => r.RoleName),
            EditEmployee = editEmployee
        };
        return View(model);
    }
}

public class EmployeeTableModel
{
    public IEnumerable<Employee> Employees { get; set; } = [];
    public Dictionary<int, string> RoleNameById { get; set; } = [];
    public Employee? EditEmployee { get; set; }
}
