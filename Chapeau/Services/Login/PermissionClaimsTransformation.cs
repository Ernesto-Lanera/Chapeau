using Chapeau.Constants.Login;
using Chapeau.Models;
using Chapeau.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Chapeau.Services.Login
{
    /// <summary>
    /// Refreshes employee and permission claims from the database on every authenticated request.
    /// Ensures deactivated employees lose access and permission changes take effect immediately.
    /// Falls back to existing cookie claims when the database is unreachable.
    /// </summary>
    public class PermissionClaimsTransformation : IClaimsTransformation
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PermissionClaimsTransformation> _logger;

        /// <summary>Initializes the transformation with repositories and logging.</summary>
        public PermissionClaimsTransformation(
            IEmployeeRepository employeeRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PermissionClaimsTransformation> logger)
        {
            _employeeRepository = employeeRepository;
            _roleRepository = roleRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>Refreshes the principal's claims from the database on every authenticated request.</summary>
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (!ShouldRefreshPermissions(principal))
            {
                return Task.FromResult(principal);
            }

            if (principal.Identity is not ClaimsIdentity currentIdentity)
            {
                return Task.FromResult(principal);
            }

            if (!TryGetEmployeeId(principal, out int employeeId))
            {
                return Task.FromResult(principal);
            }

            try
            {
                Employee? employee = _employeeRepository.GetEmployeeById(employeeId);

                if (employee is null || !employee.IsActive)
                {
                    return Task.FromResult(CreateInactivePrincipal(currentIdentity));
                }

                List<string> currentPermissions = _roleRepository.GetRolePermissions(employee.RoleID);

                var refreshedIdentity = new ClaimsIdentity(currentIdentity);
                RefreshEmployeeClaims(refreshedIdentity, employee);
                ReplacePermissionClaims(refreshedIdentity, currentPermissions);

                return Task.FromResult(new ClaimsPrincipal(refreshedIdentity));
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Employee/permission claims could not be refreshed for employee {EmployeeId}.", employeeId);
                return Task.FromResult(principal);
            }
        }

        /// <summary>Determines whether the permission claims should be refreshed, skipping static files.</summary>
        private bool ShouldRefreshPermissions(ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            string? path = _httpContextAccessor.HttpContext?.Request.Path.Value;

            return string.IsNullOrWhiteSpace(path) || !Path.HasExtension(path);
        }

        /// <summary>Extracts the employee ID from claims, checking custom and standard claim types.</summary>
        private static bool TryGetEmployeeId(ClaimsPrincipal principal, out int employeeId)
        {
            string? employeeIdValue = principal.FindFirst(ClaimTypeConstants.EmployeeId)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(employeeIdValue, out employeeId);
        }

        /// <summary>Creates a principal with IsActive=false and no permissions for deactivated employees.</summary>
        private static ClaimsPrincipal CreateInactivePrincipal(ClaimsIdentity currentIdentity)
        {
            var refreshedIdentity = new ClaimsIdentity(currentIdentity);

            ReplaceClaim(refreshedIdentity, ClaimTypeConstants.IsActive, bool.FalseString);
            ReplacePermissionClaims(refreshedIdentity, Array.Empty<string>());

            return new ClaimsPrincipal(refreshedIdentity);
        }

        /// <summary>Updates all employee-related claims with fresh data from the database.</summary>
        private static void RefreshEmployeeClaims(ClaimsIdentity identity, Employee employee)
        {
            string roleName = string.IsNullOrWhiteSpace(employee.Role.RoleName)
                ? employee.RoleName
                : employee.Role.RoleName;

            ReplaceClaim(identity, ClaimTypes.NameIdentifier, employee.EmployeeID.ToString());
            ReplaceClaim(identity, ClaimTypes.Name, employee.Name ?? string.Empty);
            ReplaceClaim(identity, ClaimTypes.GivenName, employee.Name ?? string.Empty);
            ReplaceClaim(identity, ClaimTypes.Role, roleName);
            ReplaceClaim(identity, ClaimTypeConstants.EmployeeId, employee.EmployeeID.ToString());
            ReplaceClaim(identity, ClaimTypeConstants.EmployeeName, employee.Name ?? string.Empty);
            ReplaceClaim(identity, ClaimTypeConstants.RoleId, employee.RoleID.ToString());
            ReplaceClaim(identity, ClaimTypeConstants.RoleName, roleName);
            ReplaceClaim(identity, ClaimTypeConstants.IsActive, employee.IsActive.ToString());
        }

        /// <summary>Replaces all permission claims with a fresh set from the database.</summary>
        private static void ReplacePermissionClaims(ClaimsIdentity identity, IEnumerable<string> permissions)
        {
            foreach (Claim claim in identity.FindAll(ClaimTypeConstants.Permission).ToList())
            {
                identity.RemoveClaim(claim);
            }

            foreach (string permission in permissions.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                identity.AddClaim(new Claim(ClaimTypeConstants.Permission, permission));
            }
        }

        /// <summary>Replaces all claims of a given type with a single new value.</summary>
        private static void ReplaceClaim(ClaimsIdentity identity, string claimType, string value)
        {
            foreach (Claim claim in identity.FindAll(claimType).ToList())
            {
                identity.RemoveClaim(claim);
            }

            identity.AddClaim(new Claim(claimType, value));
        }
    }
}
