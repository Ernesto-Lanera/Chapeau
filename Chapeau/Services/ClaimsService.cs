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
                new Claim(ClaimTypes.Name, employee.Username),
                new Claim(ClaimTypes.GivenName, employee.Name),

                // Role claim for authorization (supports [Authorize(Roles = "Manager")])
                new Claim(ClaimTypes.Role, employee.Role.ToString()),

                // Custom claims
                new Claim("EmployeeID", employee.EmployeeID.ToString()),
                new Claim("EmployeeName", employee.Name),
                new Claim("IsActive", employee.IsActive.ToString())
            };

            // Load and add role-specific permissions from database
            // If database lookup fails, use fallback permissions based on role
            try
            {
                var roleId = GetRoleIdFromRole(employee.Role);
                var permissions = _roleRepository.GetRolePermissions(roleId);

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
                    _logger.LogWarning("No permissions found in database for role {Role}, using defaults", employee.Role);
                    AddDefaultPermissionsForRole(claims, employee.Role);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load permissions from database for employee {EmployeeID}, using fallback", 
                    employee.EmployeeID);
                // Fallback to default permissions if database lookup fails
                AddDefaultPermissionsForRole(claims, employee.Role);
            }

            return claims;
        }

        /// <summary>
        /// Fallback method that adds permissions based on role when database is unavailable.
        /// </summary>
        private void AddDefaultPermissionsForRole(List<Claim> claims, EmployeeRole role)
        {
            List<string> permissions = role switch
            {
                EmployeeRole.Waiter => new List<string> { "ViewMenu", "TakeOrders" },
                EmployeeRole.Kitchen => new List<string> { "ViewMenu", "PrepareFood" },
                EmployeeRole.Manager => new List<string> 
                { 
                    "ViewMenu", "TakeOrders", "PrepareFood", 
                    "ManageEmployees", "ManageMenuItems", "ViewReports", "ManageRoles" 
                },
                _ => new List<string> { "ViewMenu" }
            };

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
                _logger.LogDebug("Added default permission claim: {Permission} for role {Role}", 
                    permission, role);
            }
        }

        /// <summary>
        /// Maps EmployeeRole enum to RoleID in database.
        /// </summary>
        private int GetRoleIdFromRole(EmployeeRole role)
        {
            return role switch
            {
                EmployeeRole.Waiter => 1,
                EmployeeRole.Kitchen => 2,
                EmployeeRole.Manager => 3,
                _ => 1 // Default to Waiter
            };
        }
    }
}
