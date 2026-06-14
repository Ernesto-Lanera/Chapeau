using Chapeau.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    public class PermissionController : Controller
    {
        // ===== ROLE-BASED AUTHORIZATION =====

        [Authorize(Roles = "Manager")]
        public IActionResult ManagerOnly()
        {
            return View();
        }

        [Authorize(Roles = "Waiter,Kitchen")]
        public IActionResult StaffOnly()
        {
            return View();
        }

        // ===== PERMISSION-BASED AUTHORIZATION =====

        [Authorize(Policy = "CanManageEmployees")]
        public IActionResult ManageEmployees()
        {
            return View();
        }

        [Authorize(Policy = "CanTakeOrders")]
        public IActionResult TakeOrders()
        {
            return View();
        }

        [Authorize(Policy = "CanPrepareFood")]
        public IActionResult KitchenOrders()
        {
            return View();
        }

        [Authorize(Policy = "CanManageMenuItems")]
        public IActionResult ManageMenuItems()
        {
            return View();
        }

        [Authorize(Policy = "CanViewReports")]
        public IActionResult ViewReports()
        {
            return View();
        }

        [Authorize(Policy = "CanManageRoles")]
        public IActionResult ManageRoles()
        {
            return View();
        }

        // ===== POLICY-BASED AUTHORIZATION (USING POLICIES) =====

        [Authorize(Policy = "IsManager")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

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

        [Authorize]
        public IActionResult Dashboard()
        {
            var user = User;

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

        // ===== COMPLEX AUTHORIZATION SCENARIOS =====

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
