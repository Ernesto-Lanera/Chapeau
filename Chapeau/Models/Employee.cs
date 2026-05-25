using System.ComponentModel.DataAnnotations;
using Chapeau.Constants;

namespace Chapeau.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = ErrorMessages.EmployeeNameRequired)]
        [StringLength(100, ErrorMessage = ErrorMessages.EmployeeNameTooLong)]
        public string Name { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = ErrorMessages.EmployeeRoleRequired)]
        public int RoleID { get; set; }

        public EmployeeRole Role { get; set; } = new();

        public string RoleName
        {
            get => Role.RoleName;
            set => Role.RoleName = value;
        }

        public bool IsActive { get; set; } = true;

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public void AssignRole(EmployeeRole role)
        {
            ArgumentNullException.ThrowIfNull(role);
            Role = role;
            RoleID = role.RoleID;
        }
    }
}
