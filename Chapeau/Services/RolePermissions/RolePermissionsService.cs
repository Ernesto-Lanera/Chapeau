using Chapeau.Constants.Login;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.ViewModels;

namespace Chapeau.Services
{
    public class RolePermissionsService : IRolePermissionsService
    {
        private readonly IRoleRepository _roleRepository;

        private static readonly List<string> DefaultPermissions = new List<string>
        {
            PermissionConstants.TakeOrders,
            PermissionConstants.PrepareFood,
            PermissionConstants.PrepareDrinks,
            PermissionConstants.ManageEmployees,
            PermissionConstants.ManageMenuItems,
            PermissionConstants.ManageStock,
            PermissionConstants.ViewFinance,
            PermissionConstants.ManageRoles
        };

        public RolePermissionsService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public RolePermissionsIndexViewModel GetOverview()
        {
            List<EmployeeRole> roles = _roleRepository.GetRoles();
            RolePermissionsIndexViewModel viewModel = new RolePermissionsIndexViewModel();

            foreach (EmployeeRole role in roles)
            {
                viewModel.Roles.Add(new RolePermissionRowViewModel
                {
                    RoleID = role.RoleID,
                    RoleName = role.RoleName,
                    Permissions = GetVisiblePermissions(_roleRepository.GetRolePermissions(role.RoleID))
                });
            }

            return viewModel;
        }

        public RolePermissionsEditViewModel? GetEditViewModel(int roleId)
        {
            EmployeeRole? role = GetRole(roleId);
            if (role == null)
            {
                return null;
            }

            return new RolePermissionsEditViewModel
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName,
                AllPermissions = GetAllKnownPermissions(),
                CurrentPermissions = GetVisiblePermissions(_roleRepository.GetRolePermissions(roleId))
            };
        }

        public RolePermissionsSaveResult SavePermissions(int roleId, List<string>? selectedPermissions)
        {
            EmployeeRole? role = GetRole(roleId);
            if (role == null)
            {
                return new RolePermissionsSaveResult
                {
                    Success = false,
                    ErrorMessage = "Rol niet gevonden."
                };
            }

            List<string> permissionsToSave = GetVisiblePermissions(selectedPermissions ?? new List<string>());
            _roleRepository.SetRolePermissions(roleId, permissionsToSave);

            return new RolePermissionsSaveResult
            {
                Success = true,
                RoleName = role.RoleName,
                SavedPermissions = permissionsToSave
            };
        }

        private EmployeeRole? GetRole(int roleId)
        {
            return _roleRepository.GetRoles()
                .FirstOrDefault(role => role.RoleID == roleId);
        }

        private List<string> GetAllKnownPermissions()
        {
            List<string> permissionsFromDatabase = _roleRepository.GetAllPermissionNames();

            return GetVisiblePermissions(
                permissionsFromDatabase.Union(DefaultPermissions, StringComparer.OrdinalIgnoreCase));
        }

        private static List<string> GetVisiblePermissions(IEnumerable<string> permissions)
        {
            return permissions
                .Where(IsVisiblePermission)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(permission => permission)
                .ToList();
        }

        private static bool IsVisiblePermission(string permission)
        {
            return !string.Equals(permission, PermissionConstants.LegacyViewReports, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(permission, PermissionConstants.LegacyViewMenu, StringComparison.OrdinalIgnoreCase);
        }
    }
}
