using Chapeau.Constants.Login;
using Chapeau.Models;
using Chapeau.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Chapeau.Services.Login
{
    public class PermissionClaimsTransformation : IClaimsTransformation
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PermissionClaimsTransformation> _logger;

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

                // Clone first. Do not mutate the original cookie identity directly.
                var refreshedIdentity = new ClaimsIdentity(currentIdentity);
                RefreshEmployeeClaims(refreshedIdentity, employee);
                ReplacePermissionClaims(refreshedIdentity, currentPermissions);

                return Task.FromResult(new ClaimsPrincipal(refreshedIdentity));
            }
            catch (Exception exception)
            {
                // Keep the existing claims from the login cookie when the database cannot be reached.
                _logger.LogWarning(exception, "Employee/permission claims could not be refreshed for employee {EmployeeId}.", employeeId);
                return Task.FromResult(principal);
            }
        }

        private bool ShouldRefreshPermissions(ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            string? path = _httpContextAccessor.HttpContext?.Request.Path.Value;

            // Do not query the database for css/js/images and other static files.
            return string.IsNullOrWhiteSpace(path) || !Path.HasExtension(path);
        }

        private static bool TryGetEmployeeId(ClaimsPrincipal principal, out int employeeId)
        {
            string? employeeIdValue = principal.FindFirst(ClaimTypeConstants.EmployeeId)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(employeeIdValue, out employeeId);
        }

        private static ClaimsPrincipal CreateInactivePrincipal(ClaimsIdentity currentIdentity)
        {
            var refreshedIdentity = new ClaimsIdentity(currentIdentity);

            ReplaceClaim(refreshedIdentity, ClaimTypeConstants.IsActive, bool.FalseString);
            ReplacePermissionClaims(refreshedIdentity, Array.Empty<string>());

            return new ClaimsPrincipal(refreshedIdentity);
        }

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
