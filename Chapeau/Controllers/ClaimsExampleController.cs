using Chapeau.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    /// <summary>
    /// Example controller showing how to use claims for authorization and accessing user information.
    /// </summary>
    public class ClaimsExampleController : Controller
    {
        // ===== BASIC AUTHENTICATION CHECKS =====

        /// <summary>
        /// Only accessible to authenticated users.
        /// </summary>
        [Authorize]
        public IActionResult AuthorizedAction()
        {
            // Get user information from claims
            var employeeId = User.GetEmployeeId();
            var employeeName = User.GetEmployeeName();
            var username = User.GetUsername();
            var role = User.GetRole();
            var isActive = User.IsActive();

            ViewData["EmployeeId"] = employeeId;
            ViewData["EmployeeName"] = employeeName;
            ViewData["Username"] = username;
            ViewData["Role"] = role;
            ViewData["IsActive"] = isActive;

            return View();
        }

        // ===== ROLE-BASED AUTHORIZATION =====

        /// <summary>
        /// Only accessible to managers.
        /// </summary>
        [Authorize(Roles = "Manager")]
        public IActionResult ManagerOnly()
        {
            return View();
        }

        /// <summary>
        /// Only accessible to waiters or kitchen staff.
        /// </summary>
        [Authorize(Roles = "Waiter,Kitchen")]
        public IActionResult WaiterOrKitchen()
        {
            return View();
        }

        // ===== CONDITIONAL AUTHORIZATION IN CODE =====

        /// <summary>
        /// Check claims dynamically within an action.
        /// </summary>
        [Authorize]
        public IActionResult DynamicAuthorization()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get user info
            var employeeId = User.GetEmployeeId();
            var isManager = User.HasRole("Manager");

            if (isManager)
            {
                // Show manager dashboard
                return View("ManagerDashboard");
            }
            else
            {
                // Show employee dashboard
                return View("EmployeeDashboard");
            }
        }

        /// <summary>
        /// Example showing usage of multiple role checks.
        /// </summary>
        [Authorize]
        public IActionResult RoleCheckExample()
        {
            var user = User;

            // Single role check
            if (user.HasRole("Manager"))
            {
                // Allow manager operations
            }

            // Multiple roles - user must have ONE of these roles
            if (user.HasAnyRole("Manager", "Waiter"))
            {
                // Allow managers and waiters
            }

            // User must have ALL of these roles
            if (user.HasAllRoles("Manager", "Waiter"))
            {
                // This would be rare, but possible for multi-role users
            }

            return View();
        }

        // ===== ACCESSING CLAIMS IN VIEWS =====

        /// <summary>
        /// Example showing how to access claims from a view.
        /// In your .cshtml file, you can use:
        /// 
        /// @using Chapeau.Extensions
        /// 
        /// @if (User.Identity?.IsAuthenticated ?? false)
        /// {
        ///     <p>Welcome, @User.GetEmployeeName()!</p>
        ///     <p>Role: @User.GetRole()</p>
        ///     
        ///     @if (User.HasRole("Manager"))
        ///     {
        ///         <a href="/reports">View Reports</a>
        ///     }
        /// }
        /// </summary>
        public IActionResult ViewAuthorizationExample()
        {
            return View();
        }

        // ===== ACCESSING CLAIMS IN RAZOR PAGES =====

        /// <summary>
        /// If using Razor Pages, you can access claims like this:
        /// 
        /// @using Chapeau.Extensions
        /// @if (User.HasRole("Manager"))
        /// {
        ///     <p>Only managers can see this</p>
        /// }
        /// 
        /// Or in code-behind (PageModel):
        /// public void OnGet()
        /// {
        ///     var employeeId = User.GetEmployeeId();
        ///     var role = User.GetRole();
        /// }
        /// </summary>
        public IActionResult RazorPagesExample()
        {
            return View();
        }
    }
}
