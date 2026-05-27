using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class EmployeeManagementViewModel
    {
        public IReadOnlyList<Employee> Employees { get; init; } = Array.Empty<Employee>();
        public IReadOnlyList<EmployeeRole> Roles { get; init; } = Array.Empty<EmployeeRole>();
        public Employee? EditEmployee { get; init; }
        public bool ShowCreate { get; init; }
    }

    public class EmployeeInputModel
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleID { get; set; }
    }
}
