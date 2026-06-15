using System.ComponentModel.DataAnnotations;
using Chapeau.Constants;

namespace Chapeau.Models
{
    /// <summary>
    /// Represents an employee in the system with authentication and role assignment.
    /// </summary>
    public class Employee
    {
        /// <summary>Unique identifier for the employee.</summary>
        public int EmployeeID { get; set; }

        /// <summary>The employee's display name, used as login identifier.</summary>
        [Required(ErrorMessage = ErrorMessages.EmployeeNameRequired)]
        [StringLength(100, ErrorMessage = ErrorMessages.EmployeeNameTooLong)]
        public string Name { get; set; } = string.Empty;

        /// <summary>The PBKDF2-SHA256 hashed password.</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Foreign key to the employee's assigned role.</summary>
        [Range(1, int.MaxValue, ErrorMessage = ErrorMessages.EmployeeRoleRequired)]
        public int RoleID { get; set; }

        /// <summary>Navigation property for the employee's role.</summary>
        public EmployeeRole Role { get; set; } = new();

        /// <summary>Gets or sets the role display name via the navigation property.</summary>
        public string RoleName
        {
            get => Role.RoleName;
            set => Role.RoleName = value;
        }

        /// <summary>Whether the employee account is active. Inactive accounts cannot log in.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Marks the employee as active.</summary>
        public void Activate() => IsActive = true;
        /// <summary>Marks the employee as inactive.</summary>
        public void Deactivate() => IsActive = false;

        /// <summary>Assigns a new role to the employee, updating both the navigation property and foreign key.</summary>
        public void AssignRole(EmployeeRole role)
        {
            ArgumentNullException.ThrowIfNull(role);
            Role = role;
            RoleID = role.RoleID;
        }
    }
}
