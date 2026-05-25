using Chapeau.Constants.Login;
using Chapeau.Extensions;
using Chapeau.Models;
using Chapeau.Services.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    /// <summary>
    /// Account controller for authentication (login/logout) and authorization.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IClaimsService _claimsService;
        private readonly IDashboardRouterService _dashboardRouter;

        public AccountController(
            IAuthService authService,
            IClaimsService claimsService,
            IDashboardRouterService dashboardRouter)
        {
            _authService = authService;
            _claimsService = claimsService;
            _dashboardRouter = dashboardRouter;
        }

        /// <summary>
        /// Display login form. Redirects to dashboard if already authenticated.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                var controller = _dashboardRouter.GetDashboardControllerFromClaim(
                    User.FindFirst(ClaimTypeConstants.RoleId)?.Value);

                return RedirectToAction("Index", controller);
            }

            return View(new Models.Login.LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        /// <summary>
        /// Authenticate user credentials and establish session.
        /// </summary>
        public async Task<IActionResult> Login(Models.Login.LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            AuthenticationResult authentication = await _authService.AuthenticateAsync(model.Name, model.Password);

            if (authentication.Status != AuthenticationStatus.Success || authentication.Employee is null)
            {
                model.ErrorMessage = authentication.Status == AuthenticationStatus.InactiveAccount
                    ? AuthConstants.InactiveAccountError
                    : AuthConstants.InvalidCredentialsError;

                ModelState.AddModelError(string.Empty, model.ErrorMessage);
                return View(model);
            }

            Employee employee = authentication.Employee;
            await SignInUserAsync(employee, model.RememberMe);

            if (IsValidReturnUrl(model.ReturnUrl)) return Redirect(model.ReturnUrl);

            var controller = _dashboardRouter.GetDashboardController(employee.RoleID);
            return RedirectToAction("Index", controller);
        }

        private async Task SignInUserAsync(Employee employee, bool isPersistent)
        {
            ArgumentNullException.ThrowIfNull(employee);

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

        /// <summary>
        /// Sign out user and end session.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Display access denied message.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
