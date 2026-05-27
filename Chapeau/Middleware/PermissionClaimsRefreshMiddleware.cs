using Chapeau.Constants.Login;
using Chapeau.Repositories;
using System.Security.Claims;

namespace Chapeau.Middleware
{
    /// Refreshes permission claims from the database for every authenticated page request.
    /// This keeps navigation links and authorization policies in sync after permissions are edited.
    public class PermissionClaimsRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionClaimsRefreshMiddleware> _logger;

        public PermissionClaimsRefreshMiddleware(
            RequestDelegate next,
            ILogger<PermissionClaimsRefreshMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IRoleRepository roleRepository)
        {
            if (ShouldRefreshPermissions(context) &&
                context.User.Identity is ClaimsIdentity identity &&
                int.TryParse(context.User.FindFirst(ClaimTypeConstants.RoleId)?.Value, out int roleId))
            {
                try
                {
                    // Load first, so existing login claims remain available when the database cannot be reached.
                    var currentPermissions = roleRepository.GetRolePermissions(roleId);

                    foreach (var claim in identity.FindAll(ClaimTypeConstants.Permission).ToList())
                    {
                        identity.RemoveClaim(claim);
                    }

                    foreach (string permission in currentPermissions.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        identity.AddClaim(new Claim(ClaimTypeConstants.Permission, permission));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Permission claims could not be refreshed for role {RoleId}.", roleId);
                }
            }

            await _next(context);
        }

        private static bool ShouldRefreshPermissions(HttpContext context)
        {
            return context.User.Identity?.IsAuthenticated == true &&
                   !Path.HasExtension(context.Request.Path.Value);
        }
    }
}
