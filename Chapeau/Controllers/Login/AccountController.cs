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
                var controller = _dashboardRouter.GetDashboardControllerFromClaim(
                    User.FindFirst(ClaimTypeConstants.RoleId)?.Value);

                return RedirectToAction("Index", controller);
            }

            return View(new Models.Login.LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(Models.Login.LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var employee = await _authService.AuthenticateAsync(model.Name, model.Password);

            if (employee == null)
            {
                model.ErrorMessage = AuthConstants.InvalidCredentialsError;
                ModelState.AddModelError(string.Empty, model.ErrorMessage);
                return View(model);
            }

            await SignInUserAsync(employee, model.RememberMe);

            if (IsValidReturnUrl(model.ReturnUrl)) return Redirect(model.ReturnUrl);

            var controller = _dashboardRouter.GetDashboardController(employee.RoleID);
            return RedirectToAction("Index", controller);
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
