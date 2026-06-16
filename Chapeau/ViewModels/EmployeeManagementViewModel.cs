using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class EmployeeManagementViewModel
    {
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public List<EmployeeRole> Roles { get; set; } = new List<EmployeeRole>();
        public Employee? EditEmployee { get; set; }
        public bool ShowCreate { get; set; }
    }

    public class EmployeeInputModel
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleID { get; set; }
    }
}
