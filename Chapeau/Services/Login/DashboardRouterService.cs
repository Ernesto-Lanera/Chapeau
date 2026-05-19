using Chapeau.Constants.Login;

namespace Chapeau.Services.Login
{
    public interface IDashboardRouterService
    {
        string GetDashboardController(int roleId);
        string GetDashboardControllerFromClaim(string? roleIdClaim);
    }

    public class DashboardRouterService : IDashboardRouterService
    {
        public string GetDashboardController(int roleId)
        {
            return roleId switch
            {
                RoleConstants.ManagerId => "ManageMenu",
                RoleConstants.BedieningId => "Menu",
                RoleConstants.KeukenId => "Kitchen",
                RoleConstants.BarmanId => "Bar",
                _ => "Home"
            };
        }

        public string GetDashboardControllerFromClaim(string? roleIdClaim)
        {
            if (int.TryParse(roleIdClaim, out int roleId))
            {
                return GetDashboardController(roleId);
            }

            return "Home";
        }
    }
}
