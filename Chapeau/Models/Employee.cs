namespace Chapeau.Models
{
    public enum EmployeeRole
    {
        Manager,
        Waiter,
        Kitchen,
        Bartender
    }

    public class Employee
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; } = "";
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public EmployeeRole Role { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
