using Chapeau.Models;

namespace Chapeau.Repositories
{
    /// <summary>
    /// Repository interface for role and permission data access.
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>Gets all roles ordered by name.</summary>
        List<EmployeeRole> GetRoles();
        /// <summary>Gets the list of permission names assigned to a specific role.</summary>
        List<string> GetRolePermissions(int roleId);
        /// <summary>Gets all known permission names from the database and known defaults.</summary>
        List<string> GetAllPermissionNames();
        /// <summary>Replaces all permission assignments for a role with a new set.</summary>
        void SetRolePermissions(int roleId, List<string> permissionNames);
    }
}
