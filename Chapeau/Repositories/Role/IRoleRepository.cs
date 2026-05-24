using Chapeau.Models;

namespace Chapeau.Repositories
{
    public interface IRoleRepository
    {
        List<EmployeeRole> GetRoles();
        List<string> GetRolePermissions(int roleId);
        List<string> GetAllPermissionNames();
        void SetRolePermissions(int roleId, List<string> permissionNames);
    }
}
