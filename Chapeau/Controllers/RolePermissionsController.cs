using Chapeau.Constants;
using Chapeau.Constants.Login;
using Chapeau.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanManageRoles")]
    public class RolePermissionsController(IRoleRepository roleRepository) : Controller
    {
        private readonly IRoleRepository _roleRepository = roleRepository;

        private const string FlashErrorKey = "FlashError";
        private const string FlashSuccessKey = "FlashSuccess";

        public IActionResult Index()
        {
            var roles = _roleRepository.GetRoles();
            var rolePermissions = roles.ToDictionary(r => r.RoleID, r => _roleRepository.GetRolePermissions(r.RoleID));

            ViewBag.RolePermissions = rolePermissions;
            return View(roles);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var roles = _roleRepository.GetRoles();
            var role = roles.FirstOrDefault(r => r.RoleID == id);

            if (role == null)
            {
                TempData[FlashErrorKey] = "Rol niet gevonden.";
                return RedirectToAction(nameof(Index));
            }

            var allPermissions = GetAllKnownPermissions();
            var currentPermissions = _roleRepository.GetRolePermissions(id);

            ViewBag.Role = role;
            ViewBag.AllPermissions = allPermissions;
            ViewBag.CurrentPermissions = currentPermissions;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, List<string> selectedPermissions)
        {
            var roles = _roleRepository.GetRoles();
            var role = roles.FirstOrDefault(r => r.RoleID == id);

            if (role == null)
            {
                TempData[FlashErrorKey] = "Rol niet gevonden.";
                return RedirectToAction(nameof(Index));
            }

            var permissionsToSave = selectedPermissions ?? new List<string>();

            try
            {
                _roleRepository.SetRolePermissions(id, permissionsToSave);
                TempData[FlashSuccessKey] = $"Permissies voor '{role.RoleName}' bijgewerkt. De navigatie gebruikt direct de nieuwe rechten.";
            }
            catch
            {
                TempData[FlashErrorKey] = "Er is een fout opgetreden bij het opslaan van de permissies.";
                return RedirectToAction(nameof(Index));
            }

            bool changedOwnRole = User.HasClaim(ClaimTypeConstants.RoleId, id.ToString());
            bool stillCanManageRoles = permissionsToSave.Contains(
                PermissionConstants.ManageRoles,
                StringComparer.OrdinalIgnoreCase);

            if (changedOwnRole && !stillCanManageRoles)
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction(nameof(Index));
        }

        private List<string> GetAllKnownPermissions()
        {
            var dbPermissions = _roleRepository.GetAllPermissionNames();
            var defaultPermissions = new List<string>
            {
                PermissionConstants.ViewMenu,
                PermissionConstants.TakeOrders,
                PermissionConstants.PrepareFood,
                PermissionConstants.PrepareDrinks,
                PermissionConstants.ManageEmployees,
                PermissionConstants.ManageMenuItems,
                PermissionConstants.ManageStock,
                PermissionConstants.ViewFinance,
                PermissionConstants.ManageRoles
            };

            return dbPermissions
                .Where(permission => permission != PermissionConstants.LegacyViewReports)
                .Union(defaultPermissions)
                .OrderBy(permission => permission)
                .ToList();
        }
    }
}
