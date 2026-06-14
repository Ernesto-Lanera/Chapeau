using System.Security.Claims;

namespace Chapeau.Extensions
{
    /// <summary>
    /// Extension methods for accessing claims from ClaimsPrincipal.
    /// Makes it easier to work with claims throughout your application.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the employee ID from claims.
        /// </summary>
        public static int GetEmployeeId(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst("EmployeeID");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        /// <summary>
        /// Gets the employee name from claims.
        /// </summary>
        public static string GetEmployeeName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("EmployeeName")?.Value ?? string.Empty;
        }

        /// <summary>
        /// Gets the username from claims (standard claim).
        /// </summary>
        public static string GetUsername(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Gets the employee role from claims.
        /// </summary>
        public static string GetRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Checks if the employee is active based on claims.
        /// </summary>
        public static bool IsActive(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst("IsActive");
            return claim != null && bool.TryParse(claim.Value, out var isActive) && isActive;
        }

        // ===== ROLE CHECKS =====

        /// <summary>
        /// Checks if the user has a specific role.
        /// </summary>
        public static bool HasRole(this ClaimsPrincipal principal, string role)
        {
            return principal.IsInRole(role);
        }

        /// <summary>
        /// Checks if the user has any of the specified roles.
        /// </summary>
        public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
        {
            return roles.Any(role => principal.IsInRole(role));
        }

        /// <summary>
        /// Checks if the user has all of the specified roles.
        /// </summary>
        public static bool HasAllRoles(this ClaimsPrincipal principal, params string[] roles)
        {
            return roles.All(role => principal.IsInRole(role));
        }

        // ===== PERMISSION CHECKS =====

        /// <summary>
        /// Checks if the user has a specific permission.
        /// Permissions are loaded from the database based on the user's role.
        /// </summary>
        public static bool HasPermission(this ClaimsPrincipal principal, string permission)
        {
            return principal.FindAll("Permission").Any(c => c.Value == permission);
        }

        /// <summary>
        /// Checks if the user has any of the specified permissions.
        /// </summary>
        public static bool HasAnyPermission(this ClaimsPrincipal principal, params string[] permissions)
        {
            var userPermissions = principal.FindAll("Permission").Select(c => c.Value).ToList();
            return permissions.Any(p => userPermissions.Contains(p));
        }

        /// <summary>
        /// Checks if the user has all of the specified permissions.
        /// </summary>
        public static bool HasAllPermissions(this ClaimsPrincipal principal, params string[] permissions)
        {
            var userPermissions = principal.FindAll("Permission").Select(c => c.Value).ToList();
            return permissions.All(p => userPermissions.Contains(p));
        }

        /// <summary>
        /// Gets all permissions for the user.
        /// </summary>
        public static List<string> GetPermissions(this ClaimsPrincipal principal)
        {
            return principal.FindAll("Permission").Select(c => c.Value).ToList();
        }

        // ===== COMMON PERMISSION SHORTCUTS =====

        public static bool CanTakeOrders(this ClaimsPrincipal principal) => principal.HasPermission("TakeOrders");
        public static bool CanPrepareFood(this ClaimsPrincipal principal) => principal.HasPermission("PrepareFood");
        public static bool CanPrepareDrinks(this ClaimsPrincipal principal) => principal.HasPermission("PrepareDrinks");
        public static bool CanManageEmployees(this ClaimsPrincipal principal) => principal.HasPermission("ManageEmployees");
        public static bool CanManageMenuItems(this ClaimsPrincipal principal) => principal.HasPermission("ManageMenuItems");
        public static bool CanManageStock(this ClaimsPrincipal principal) => principal.HasPermission("ManageStock");
        public static bool CanViewFinance(this ClaimsPrincipal principal) => principal.HasPermission("ViewFinance");
        public static bool CanViewReports(this ClaimsPrincipal principal) => principal.HasPermission("ViewReports");
        public static bool CanManageRoles(this ClaimsPrincipal principal) => principal.HasPermission("ManageRoles");
        public static bool CanViewMenu(this ClaimsPrincipal principal) => principal.HasPermission("ViewMenu");
    }
}
