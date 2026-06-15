using Chapeau.Constants.Login;
using System.Security.Claims;

namespace Chapeau.Services.Login
{
    /// <summary>
    /// Determines the appropriate dashboard controller to redirect to after login.
    /// </summary>
    public interface IDashboardRouterService
    {
        /// <summary>Returns the controller name for the user's default dashboard based on their role and permissions.</summary>
        string GetDashboardController(ClaimsPrincipal principal);
    }

    /// <summary>
    /// Routes users to their default dashboard based on role and permission priority.
    /// Managers see management panels first; waiters, kitchen, and bar staff go to their respective stations.
    /// </summary>
    public class DashboardRouterService : IDashboardRouterService
    {
        /// <summary>Determines the best dashboard controller for the given principal based on role and permissions.</summary>
        public string GetDashboardController(ClaimsPrincipal principal)
        {
            int.TryParse(principal.FindFirst(ClaimTypeConstants.RoleId)?.Value, out int roleId);

            return roleId switch
            {
                RoleConstants.ManagerId => FirstAllowedController(principal,
                    (PermissionConstants.ManageMenuItems, "ManageMenu"),
                    (PermissionConstants.ManageEmployees, "Employee"),
                    (PermissionConstants.ManageStock, "Stock"),
                    (PermissionConstants.ViewFinance, "Finance"),
                    (PermissionConstants.ManageRoles, "RolePermissions"),
                    (PermissionConstants.TakeOrders, "Table"),
                    (PermissionConstants.PrepareFood, "Kitchen"),
                    (PermissionConstants.PrepareDrinks, "Bar")),

                RoleConstants.BedieningId => FirstAllowedController(principal,
                    (PermissionConstants.TakeOrders, "Table")),

                RoleConstants.KeukenId => FirstAllowedController(principal,
                    (PermissionConstants.PrepareFood, "Kitchen")),

                RoleConstants.BarmanId => FirstAllowedController(principal,
                    (PermissionConstants.PrepareDrinks, "Bar")),

                _ => FirstAllowedController(principal,
                    (PermissionConstants.TakeOrders, "Table"),
                    (PermissionConstants.PrepareFood, "Kitchen"),
                    (PermissionConstants.PrepareDrinks, "Bar"),
                    (PermissionConstants.ManageMenuItems, "ManageMenu"),
                    (PermissionConstants.ManageEmployees, "Employee"),
                    (PermissionConstants.ManageStock, "Stock"),
                    (PermissionConstants.ViewFinance, "Finance"),
                    (PermissionConstants.ManageRoles, "RolePermissions"))
            };
        }

        /// <summary>
        /// Returns the first controller name for which the principal has the required permission.
        /// Falls back to "Menu" if none of the candidates have permission.
        /// </summary>
        private static string FirstAllowedController(
            ClaimsPrincipal principal,
            params (string Permission, string Controller)[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (principal.HasClaim(ClaimTypeConstants.Permission, candidate.Permission))
                {
                    return candidate.Controller;
                }
            }

            return "Menu";
        }
    }
}
