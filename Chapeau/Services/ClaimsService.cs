using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Chapeau.Services
{
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

        public ClaimsPrincipal CreateClaimsPrincipal(Employee employee)
        {
            var claimsIdentity = new ClaimsIdentity(
                CreateClaims(employee),
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return new ClaimsPrincipal(claimsIdentity);
        }

        public List<Claim> CreateClaims(Employee employee)
        {
            string roleName = GetRoleName(employee.RoleID);

            var claims = BuildBaseClaims(employee, roleName);

            AddPermissionClaims(claims, employee);

            return claims;
        }

        private static List<Claim> BuildBaseClaims(Employee employee, string roleName)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeID.ToString()),
                new Claim(ClaimTypes.Name, employee.Name ?? string.Empty),
                new Claim(ClaimTypes.GivenName, employee.Name ?? string.Empty),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(ClaimTypeConstants.EmployeeId, employee.EmployeeID.ToString()),
                new Claim(ClaimTypeConstants.EmployeeName, employee.Name ?? string.Empty),
                new Claim(ClaimTypeConstants.RoleId, employee.RoleID.ToString()),
                new Claim(ClaimTypeConstants.RoleName, roleName),
                new Claim(ClaimTypeConstants.IsActive, employee.IsActive.ToString())
            };
        }

        private void AddPermissionClaims(List<Claim> claims, Employee employee)
        {
            try
            {
                var permissions = _roleRepository.GetRolePermissions(employee.RoleID);

                if (permissions.Any())
                {
                    AddPermissionClaimsFromList(claims, permissions, employee);
                }
                else
                {
                    _logger.LogWarning(
                        "No permissions found in database for role {RoleID}, using defaults",
                        employee.RoleID
                    );

                    AddDefaultPermissionsForRole(claims, employee.RoleID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to load permissions from database for employee {EmployeeID}, using fallback",
                    employee.EmployeeID
                );

                AddDefaultPermissionsForRole(claims, employee.RoleID);
            }
        }

        private void AddPermissionClaimsFromList(List<Claim> claims, List<string> permissions, Employee employee)
        {
            foreach (var permission in permissions)
            {
                claims.Add(new Claim(ClaimTypeConstants.Permission, permission));

                _logger.LogDebug(
                    "Added permission claim: {Permission} for employee {EmployeeID}",
                    permission,
                    employee.EmployeeID
                );
            }
        }

        private static string GetRoleName(int roleID)
        {
            return roleID switch
            {
                RoleConstants.ManagerId => RoleConstants.ManagerName,
                RoleConstants.BedieningId => RoleConstants.BedieningName,
                RoleConstants.KeukenId => RoleConstants.KeukenName,
                RoleConstants.BarmanId => RoleConstants.BarmanName,
                _ => RoleConstants.UnknownName
            };
        }

        private void AddDefaultPermissionsForRole(List<Claim> claims, int roleID)
        {
            var permissions = GetDefaultPermissions(roleID);

            foreach (var permission in permissions)
            {
                claims.Add(new Claim(ClaimTypeConstants.Permission, permission));

                _logger.LogDebug(
                    "Added default permission claim: {Permission} for role {RoleID}",
                    permission,
                    roleID
                );
            }
        }

        private static List<string> GetDefaultPermissions(int roleID)
        {
            return roleID switch
            {
                RoleConstants.ManagerId => new List<string>
                {
                    PermissionConstants.ViewMenu,
                    PermissionConstants.TakeOrders,
                    PermissionConstants.PrepareFood,
                    PermissionConstants.PrepareDrinks,
                    PermissionConstants.ManageEmployees,
                    PermissionConstants.ManageMenuItems,
                    PermissionConstants.ManageStock,
                    PermissionConstants.ViewReports,
                    PermissionConstants.ManageRoles,
                    PermissionConstants.ViewFinance
                },

                RoleConstants.BedieningId => new List<string>
                {
                    PermissionConstants.ViewMenu,
                    PermissionConstants.TakeOrders
                },

                RoleConstants.KeukenId => new List<string>
                {
                    PermissionConstants.ViewMenu,
                    PermissionConstants.PrepareFood
                },

                RoleConstants.BarmanId => new List<string>
                {
                    PermissionConstants.ViewMenu,
                    PermissionConstants.PrepareDrinks
                },

                _ => new List<string>
                {
                    PermissionConstants.ViewMenu
                }
            };
        }
    }
}
