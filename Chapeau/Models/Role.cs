namespace Chapeau.Models
{
    /// <summary>
    /// Represents a role that can be assigned to employees, governing their permissions.
    /// </summary>
    public class EmployeeRole
    {
        /// <summary>Unique identifier for the role.</summary>
        public int RoleID { get; set; }
        /// <summary>Display name of the role (e.g. Manager, Bediening, Keuken).</summary>
        public string RoleName { get; set; } = "";
    }
}