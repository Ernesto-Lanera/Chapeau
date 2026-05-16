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

        /// <summary>
        /// Creates a list of claims for the employee.
        /// Includes identity claims, role claims, and permission claims for authorization.
        /// </summary>
        public List<Claim> CreateClaims(Employee employee)
        {
            var claims = new List<Claim>
            {
                // Identity claims
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeID.ToString()),
                new Claim(ClaimTypes.Name, employee.Name),
                new Claim(ClaimTypes.GivenName, employee.Name),

                // Role claim for authorization (supports [Authorize(Roles = "Manager")])
                new Claim(ClaimTypes.Role, employee.RoleID.ToString()),

                // Custom claims
                new Claim("EmployeeID", employee.EmployeeID.ToString()),
                new Claim("EmployeeName", employee.Name),
                new Claim("IsActive", employee.IsActive.ToString())
            };

            // Load and add role-specific permissions from database
            // If database lookup fails, use fallback permissions based on role
            try
            {
                var permissions = _roleRepository.GetRolePermissions(employee.RoleID);

                if (permissions.Any())
                {
                    // Add each permission as a separate claim
                    foreach (var permission in permissions)
                    {
                        claims.Add(new Claim("Permission", permission));
                        _logger.LogDebug("Added permission claim: {Permission} for employee {EmployeeID}", 
                            permission, employee.EmployeeID);
                    }
                }
                else
                {
                    // Fallback: if no permissions found in database, use defaults
                    _logger.LogWarning("No permissions found in database for role {RoleID}, using defaults", employee.RoleID);
                    AddDefaultPermissionsForRole(claims, employee.RoleID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load permissions from database for employee {EmployeeID}, using fallback", 
                    employee.EmployeeID);
                // Fallback to default permissions if database lookup fails
                AddDefaultPermissionsForRole(claims, employee.RoleID);
            }

            return claims;
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
