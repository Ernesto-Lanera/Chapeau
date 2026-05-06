namespace Chapeau.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public int RoleID { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
