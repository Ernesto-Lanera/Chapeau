using Chapeau.Constants;
using Chapeau.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanManageRoles")]
    public class RolePermissionsController(RoleRepository roleRepository) : Controller
    {
        private readonly RoleRepository _roleRepository = roleRepository;

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

            try
            {
                _roleRepository.SetRolePermissions(id, selectedPermissions ?? new List<string>());
                TempData[FlashSuccessKey] = $"Permissies voor '{role.RoleName}' succesvol bijgewerkt.";
            }
            catch
            {
                TempData[FlashErrorKey] = "Er is een fout opgetreden bij het opslaan van de permissies.";
            }

            return RedirectToAction(nameof(Index));
        }

        private List<string> GetAllKnownPermissions()
        {
            var dbPermissions = _roleRepository.GetAllPermissionNames();
            var defaultPermissions = new List<string>
            {
                "ViewMenu",
                "TakeOrders",
                "PrepareFood",
                "PrepareDrinks",
                "ManageEmployees",
                "ManageMenuItems",
                "ManageStock",
                "ViewReports",
                "ManageRoles",
                "ViewFinance"
            };

            return dbPermissions.Count > 0
                ? dbPermissions.Union(defaultPermissions).OrderBy(p => p).ToList()
                : defaultPermissions;
        }
    }
}
