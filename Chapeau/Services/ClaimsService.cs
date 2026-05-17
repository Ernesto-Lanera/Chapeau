using Chapeau.Models;
using Chapeau.Repositories;
using System.Security.Claims;

namespace Chapeau.Services
{
    /// <summary>
    /// Service for creating and managing user claims for authentication and authorization.
    /// Includes role claims and permission claims loaded from the database.
    /// </summary>
    public interface IClaimsService
    {
        ClaimsPrincipal CreateClaimsPrincipal(Employee employee);
        List<Claim> CreateClaims(Employee employee);
    }

    public class ClaimsService : IClaimsService
    {
        private readonly RoleRepository _roleRepository;
        private readonly ILogger<ClaimsService> _logger;

        public ClaimsService(RoleRepository roleRepository, ILogger<ClaimsService> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a ClaimsPrincipal for the authenticated employee.
        /// </summary>
        public ClaimsPrincipal CreateClaimsPrincipal(Employee employee)
        {
            var claims = CreateClaims(employee);
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            return new ClaimsPrincipal(claimsIdentity);
        }

        public List<Claim> CreateClaims(Employee employee)
        {
            var claims = BuildBaseClaims(employee);
            LoadPermissionClaims(claims, employee);
            return claims;
        }

        private static List<Claim> BuildBaseClaims(Employee employee)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeID.ToString()),
                new Claim(ClaimTypes.Name, employee.Name),
                new Claim(ClaimTypes.GivenName, employee.Name),
                new Claim(ClaimTypes.Role, employee.RoleID.ToString()),
                new Claim("EmployeeID", employee.EmployeeID.ToString()),
                new Claim("EmployeeName", employee.Name),
                new Claim("IsActive", employee.IsActive.ToString())
            };
        }

        private void LoadPermissionClaims(List<Claim> claims, Employee employee)
        {
            try
            {
                var permissions = _roleRepository.GetRolePermissions(employee.RoleID);

                if (permissions.Any())
                {
                    foreach (var permission in permissions)
                    {
                        claims.Add(new Claim("Permission", permission));
                    }
                }
                else
                {
                    _logger.LogWarning("No permissions found in database for role {RoleID}, using defaults", employee.RoleID);
                    AddDefaultPermissionsForRole(claims, employee.RoleID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load permissions for employee {EmployeeID}, using fallback", employee.EmployeeID);
                AddDefaultPermissionsForRole(claims, employee.RoleID);
            }
        }

        /// <summary>
        /// Fallback method that adds permissions based on role when database is unavailable.
        /// </summary>
        private void AddDefaultPermissionsForRole(List<Claim> claims, int roleID)
        {
            List<string> permissions = roleID switch
            {
                1 => new List<string> { "ViewMenu", "TakeOrders" }, // Waiter
                2 => new List<string> { "ViewMenu", "PrepareFood" }, // Kitchen
                3 => new List<string> 
                { 
                    "ViewMenu", "TakeOrders", "PrepareFood", 
                    "ManageEmployees", "ManageMenuItems", "ViewReports", "ManageRoles" 
                }, // Manager
                _ => new List<string> { "ViewMenu" }
            };

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
                _logger.LogDebug("Added default permission claim: {Permission} for role {RoleID}", 
                    permission, roleID);
            }
        }
    }
}
