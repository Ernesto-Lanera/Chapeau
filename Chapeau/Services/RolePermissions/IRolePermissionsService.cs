using Chapeau.ViewModels;

namespace Chapeau.Services
{
    public interface IRolePermissionsService
    {
        RolePermissionsIndexViewModel GetOverview();
        RolePermissionsEditViewModel? GetEditViewModel(int roleId);
        RolePermissionsSaveResult SavePermissions(int roleId, List<string>? selectedPermissions);
    }
}
