using Chapeau.Constants.Login;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanManageRoles")]
    public class RolePermissionsController : Controller
    {
        private readonly IRolePermissionsService _rolePermissionsService;

        private const string FlashErrorKey = "FlashError";
        private const string FlashSuccessKey = "FlashSuccess";

        public RolePermissionsController(IRolePermissionsService rolePermissionsService)
        {
            _rolePermissionsService = rolePermissionsService;
        }

        public IActionResult Index()
        {
            RolePermissionsIndexViewModel viewModel = _rolePermissionsService.GetOverview();
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            RolePermissionsEditViewModel? viewModel = _rolePermissionsService.GetEditViewModel(id);
            if (viewModel == null)
            {
                TempData[FlashErrorKey] = "Rol niet gevonden.";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(int id, List<string>? selectedPermissions)
        {
            try
            {
                RolePermissionsSaveResult result = _rolePermissionsService.SavePermissions(id, selectedPermissions);
                if (!result.Success)
                {
                    TempData[FlashErrorKey] = result.ErrorMessage;
                    return RedirectToAction(nameof(Index));
                }

                TempData[FlashSuccessKey] = $"Permissies voor '{result.RoleName}' bijgewerkt.";

                if (CurrentUserCannotManageRolesAnymore(id, result.SavedPermissions))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }
            catch
            {
                TempData[FlashErrorKey] = "Er is een fout opgetreden bij het opslaan van de permissies.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CurrentUserCannotManageRolesAnymore(int changedRoleId, List<string> savedPermissions)
        {
            bool changedOwnRole = User.HasClaim(ClaimTypeConstants.RoleId, changedRoleId.ToString());
            bool stillCanManageRoles = savedPermissions.Contains(
                PermissionConstants.ManageRoles,
                StringComparer.OrdinalIgnoreCase);

            return changedOwnRole && !stillCanManageRoles;
        }
    }
}
