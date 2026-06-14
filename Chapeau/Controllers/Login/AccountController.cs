using Chapeau.Constants.Login;
using Chapeau.Extensions;
using Chapeau.Models;
using Chapeau.Services.Login;
using Chapeau.ViewModels.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chapeau.Controllers
{
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                var controller = _dashboardRouter.GetDashboardController(User);

                return RedirectToAction("Index", controller);
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
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
            var claimsPrincipal = _claimsService.CreateClaimsPrincipal(employee);
            await SignInUserAsync(claimsPrincipal, model.RememberMe);

            if (IsValidReturnUrl(model.ReturnUrl)) return Redirect(model.ReturnUrl);

            var controller = _dashboardRouter.GetDashboardController(claimsPrincipal);
            return RedirectToAction("Index", controller);
        }

        private async Task SignInUserAsync(ClaimsPrincipal claimsPrincipal, bool isPersistent)
        {
            ArgumentNullException.ThrowIfNull(claimsPrincipal);

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

        [HttpGet]
        [Authorize]
        public IActionResult Logout()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [ActionName("Logout")]
        public async Task<IActionResult> LogoutConfirmed()
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
