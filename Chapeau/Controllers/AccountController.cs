using Chapeau.Constants;
using Chapeau.Extensions;
using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IClaimsService _claimsService;

        public AccountController(IAuthService authService, IClaimsService claimsService)
        {
            _authService = authService;
            _claimsService = claimsService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated ?? false)
                return GetRedirectForAuthenticatedUser();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        private IActionResult GetRedirectForAuthenticatedUser()
        {
            foreach (var (roleId, controller) in GetRoleRedirects())
            {
                if (User.HasClaim(ClaimTypeConstants.RoleId, roleId))
                    return RedirectToAction("Index", controller);
            }

            return RedirectToAction("Index", "Home");
        }

        private static (string, string)[] GetRoleRedirects()
        {
            return new[]
            {
                (RoleConstants.ManagerId.ToString(), "ManageMenu"),
                (RoleConstants.BedieningId.ToString(), "Menu"),
                (RoleConstants.KeukenId.ToString(), "Kitchen"),
                (RoleConstants.BarmanId.ToString(), "Bar")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View(model);

            var employee = await _authService.AuthenticateAsync(model.Name, model.Password);

            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, AuthConstants.InvalidCredentialsError);
                return View(model);
            }

            await SignInUserAsync(employee, model.RememberMe);

            if (IsValidReturnUrl(returnUrl)) return Redirect(returnUrl);

            return RedirectToDashboard(employee.RoleID);
        }

        private async Task SignInUserAsync(Employee employee, bool isPersistent)
        {
            var claimsPrincipal = _claimsService.CreateClaimsPrincipal(employee);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(AuthConstants.SessionDurationHours)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties
            );
        }

        private static bool IsValidReturnUrl(string? returnUrl)
        {
            return !string.IsNullOrWhiteSpace(returnUrl)
                && Uri.TryCreate(returnUrl, UriKind.Relative, out _);
        }

        private static IActionResult RedirectToDashboard(int roleId)
        {
            string controller = roleId switch
            {
                RoleConstants.ManagerId => "ManageMenu",
                RoleConstants.BedieningId => "Menu",
                RoleConstants.KeukenId => "Kitchen",
                RoleConstants.BarmanId => "Bar",
                _ => "Home"
            };

            return new RedirectToActionResult("Index", controller, null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
