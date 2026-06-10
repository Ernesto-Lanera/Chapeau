using Chapeau.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    /// <summary>
    /// Example controller demonstrating permission-based and role-based authorization.
    /// This shows all the different ways to use claims for access control.
    /// </summary>
    public class PermissionExampleController : Controller
    {
        // ===== ROLE-BASED AUTHORIZATION =====

        /// <summary>
        /// Only accessible to authenticated managers.
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
        public IActionResult StaffOnly()
        {
            return View();
        }

        // ===== PERMISSION-BASED AUTHORIZATION =====

        /// <summary>
        /// Only accessible to users with "CanManageEmployees" permission.
        /// </summary>
        [Authorize(Policy = "CanManageEmployees")]
        public IActionResult ManageEmployees()
        {
            return View();
        }

        /// <summary>
        /// Only accessible to users with "CanTakeOrders" permission.
        /// </summary>
        [Authorize(Policy = "CanTakeOrders")]
        public IActionResult TakeOrders()
        {
            return View();
        }

        /// <summary>
        /// Only accessible to users with "CanPrepareFood" permission.
        /// </summary>
        [Authorize(Policy = "CanPrepareFood")]
        public IActionResult KitchenOrders()
        {
            return View();
        }

        /// <summary>
        /// Only accessible to users with "CanManageMenuItems" permission.
        /// </summary>
        [Authorize(Policy = "CanManageMenuItems")]
        public IActionResult ManageMenuItems()
        {
            return View();
        }

        /// <summary>
        /// Only accessible to users with "CanViewReports" permission.
        /// </summary>
        [Authorize(Policy = "CanViewReports")]
        public IActionResult ViewReports()
        {
            return View();
        }

        /// <summary>
        /// Only accessible to users with "CanManageRoles" permission.
        /// </summary>
        [Authorize(Policy = "CanManageRoles")]
        public IActionResult ManageRoles()
        {
            return View();
        }

        // ===== POLICY-BASED AUTHORIZATION (USING POLICIES) =====

        /// <summary>
        /// Uses a custom policy that requires the Manager role.
        /// </summary>
        [Authorize(Policy = "IsManager")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        /// <summary>
        /// Uses a custom policy that requires Waiter or Kitchen role.
        /// </summary>
        [Authorize(Policy = "IsWaiter")]
        public IActionResult WaiterDashboard()
        {
            return View();
        }

        [Authorize(Policy = "IsKitchenStaff")]
        public IActionResult KitchenDashboard()
        {
            return View();
        }

        // ===== CONDITIONAL AUTHORIZATION IN CODE =====

        /// <summary>
        /// Check permissions dynamically within an action.
        /// Different views for different permission levels.
        /// </summary>
        [Authorize]
        public IActionResult Dashboard()
        {
            var user = User;

            // Return different views based on permissions
            if (user.CanManageEmployees())
            {
                return View("AdminDashboard");
            }
            else if (user.CanPrepareFood())
            {
                return View("KitchenDashboard");
            }
            else if (user.CanTakeOrders())
            {
                return View("WaiterDashboard");
            }
            else
            {
                return View("DefaultDashboard");
            }
        }

        /// <summary>
        /// Multiple permission checks using AND logic.
        /// User must have ALL permissions to proceed.
        /// </summary>
        [Authorize]
        public IActionResult RequireMultiplePermissions()
        {
            if (!User.HasAllPermissions("ManageEmployees", "ViewReports"))
            {
                return Forbid();
            }

            // User has both permissions
            return View();
        }

        /// <summary>
        /// Multiple permission checks using OR logic.
        /// User must have AT LEAST ONE permission.
        /// </summary>
        [Authorize]
        public IActionResult RequireAnyPermission()
        {
            if (!User.HasAnyPermission("TakeOrders", "PrepareFood"))
            {
                return Forbid();
            }

            // User has at least one permission
            return View();
        }

        /// <summary>
        /// Check permissions and display user information.
        /// </summary>
        [Authorize]
        public IActionResult UserInfo()
        {
            var employeeId = User.GetEmployeeId();
            var employeeName = User.GetEmployeeName();
            var role = User.GetRole();
            var permissions = User.GetPermissions();

            var viewData = new Dictionary<string, object>
            {
                { "EmployeeId", employeeId },
                { "EmployeeName", employeeName },
                { "Role", role },
                { "Permissions", permissions },
                { "CanManageEmployees", User.CanManageEmployees() },
                { "CanTakeOrders", User.CanTakeOrders() },
                { "CanPrepareFood", User.CanPrepareFood() }
            };

            return View(viewData);
        }

        // ===== ROLE AND PERMISSION SHORTCUTS =====

        /// <summary>
        /// Examples using shortcut extension methods for common permissions.
        /// </summary>
        [Authorize]
        public IActionResult PermissionShortcuts()
        {
            var user = User;

            // These shortcut methods make code more readable
            if (user.CanTakeOrders())
            {
                // Show order form
            }

            if (user.CanPrepareFood())
            {
                // Show kitchen orders
            }

            if (user.CanManageEmployees())
            {
                // Show employee management
            }

            if (user.CanViewReports())
            {
                // Show reports
            }

            return View();
        }

        // ===== COMPLEX AUTHORIZATION SCENARIOS =====

        /// <summary>
        /// Manager-only feature that requires multiple checks.
        /// </summary>
        [Authorize]
        public IActionResult AdvancedFeature()
        {
            var user = User;

            // Managers can always access
            if (user.HasRole("Manager"))
            {
                return View("AdvancedFeature");
            }

            // Waiters need explicit permission
            if (user.HasRole("Waiter") && user.CanTakeOrders())
            {
                return View("AdvancedFeature");
            }

            // Kitchen staff need explicit permission
            if (user.HasRole("Kitchen") && user.CanPrepareFood())
            {
                return View("AdvancedFeature");
            }

            return Forbid();
        }

        /// <summary>
        /// Role with specific permission requirement.
        /// </summary>
        [Authorize]
        public IActionResult RestrictedOperation()
        {
            var user = User;

            // Only managers OR users with specific permission
            if (!user.HasRole("Manager") && !user.CanManageMenuItems())
            {
                return Forbid();
            }

            return View();
        }
    }
}
