using Chapeau.Constants.Login;
using System.Security.Claims;

namespace Chapeau.Services.Login
{
    public interface IDashboardRouterService
    {
        string GetDashboardController(ClaimsPrincipal principal);
    }

    public class DashboardRouterService : IDashboardRouterService
    {
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
