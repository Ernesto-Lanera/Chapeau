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
            {
                if (User.IsInRole("Manager") || User.HasClaim("RoleID", "1"))
                {
                    return RedirectToAction("Index", "ManageMenu");
                }

                if (User.IsInRole("Bediening") || User.HasClaim("RoleID", "3"))
                {
                    return RedirectToAction("Index", "Menu");
                }

                if (User.IsInRole("Keuken") || User.HasClaim("RoleID", "4"))
                {
                    return RedirectToAction("Index", "Kitchen");
                }

                if (User.IsInRole("Barman") || User.HasClaim("RoleID", "5"))
                {
                    return RedirectToAction("Index", "Bar");
                }

                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = await _authService.AuthenticateAsync(model.Name, model.Password);

            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, "Ongeldige naam of wachtwoord.");
                return View(model);
            }

            var claimsPrincipal = _claimsService.CreateClaimsPrincipal(employee);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties
            );

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl) &&
                !returnUrl.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(returnUrl);
            }

            if (employee.RoleID == 1)
            {
                return RedirectToAction("Index", "ManageMenu");
            }

            if (employee.RoleID == 3)
            {
                return RedirectToAction("Index", "Menu");
            }

            if (employee.RoleID == 4)
            {
                return RedirectToAction("Index", "Kitchen");
            }

            if (employee.RoleID == 5)
            {
                return RedirectToAction("Index", "Bar");
            }

            return RedirectToAction("Index", "Home");
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