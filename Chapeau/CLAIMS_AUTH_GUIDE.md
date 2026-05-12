# Claims-Based Authentication & Authorization Guide

## Overview

Your Chapeau application now uses a comprehensive **claims-based authentication and authorization** system. This provides:

- **Type-safe access** to user information
- **Role-based authorization** with standard ASP.NET Core attributes
- **Permission-based authorization** loaded from the database
- **Extensible claim system** for custom user data
- **Standard .NET Core patterns** aligned with best practices

## Architecture

### Services & Repositories

#### ClaimsService
Creates and manages claims for authenticated users. Located in `Chapeau\Services\ClaimsService.cs`

**Standard Claims Created:**
- `ClaimTypes.NameIdentifier` - Employee ID
- `ClaimTypes.Name` - Username
- `ClaimTypes.GivenName` - Employee Name
- `ClaimTypes.Role` - Employee Role (Manager, Waiter, Kitchen)

**Custom Claims Created:**
- `EmployeeID` - Numeric employee identifier
- `EmployeeName` - Full employee name
- `IsActive` - Whether employee is active
- `Permission` - Permissions loaded from database (multiple claims, one per permission)

#### RoleRepository
Loads role and permission information from the database. Located in `Chapeau\Repositories\RoleRepository.cs`

### Authentication & Authorization Flow

#### Login Flow
1. User submits username and password
2. `AuthService.AuthenticateAsync()` validates credentials and returns `Employee` object
3. `AccountController` calls `ClaimsService.CreateClaimsPrincipal()`
4. `ClaimsService` creates list of claims including:
   - User identity claims
   - Role claim
   - Permission claims (loaded from `RolePermissions` table via `RoleRepository`)
5. `HttpContext.SignInAsync()` creates cookie with claims
6. User is authenticated and authorized

#### Logout Flow
1. User clicks logout button
2. `HttpContext.SignOutAsync()` clears authentication cookie
3. User is redirected to home page

### Authorization Policies

Authorization policies are configured in `Program.cs`:

**Permission-Based Policies:**
- `CanViewMenu` - Requires "ViewMenu" permission
- `CanTakeOrders` - Requires "TakeOrders" permission
- `CanPrepareFood` - Requires "PrepareFood" permission
- `CanManageEmployees` - Requires "ManageEmployees" permission
- `CanManageMenuItems` - Requires "ManageMenuItems" permission
- `CanViewReports` - Requires "ViewReports" permission
- `CanManageRoles` - Requires "ManageRoles" permission

**Role-Based Policies:**
- `IsManager` - Requires "Manager" role
- `IsWaiter` - Requires "Waiter" role
- `IsKitchenStaff` - Requires "Kitchen" role

## Usage Examples

### In Controllers

#### Basic Authentication Check
```csharp
using Chapeau.Extensions;

[Authorize]
public IActionResult MyAction()
{
    // Get user information from claims
    int employeeId = User.GetEmployeeId();
    string employeeName = User.GetEmployeeName();
    string role = User.GetRole();
    bool isActive = User.IsActive();

    return View();
}
```

#### Role-Based Authorization
```csharp
// Only managers can access
[Authorize(Roles = "Manager")]
public IActionResult ManagerDashboard()
{
    return View();
}

// Managers or waiters
[Authorize(Roles = "Manager,Waiter")]
public IActionResult StaffArea()
{
    return View();
}
```

#### Permission-Based Authorization
```csharp
// Only users with "ManageEmployees" permission
[Authorize(Policy = "CanManageEmployees")]
public IActionResult ManageEmployees()
{
    return View();
}

// Only users who can prepare food
[Authorize(Policy = "CanPrepareFood")]
public IActionResult KitchenOrders()
{
    return View();
}
```

#### Conditional Authorization in Code
```csharp
[Authorize]
public IActionResult SomeAction()
{
    // Role checks
    if (User.HasRole("Manager"))
    {
        // Show manager options
    }
    else if (User.HasAnyRole("Waiter", "Kitchen"))
    {
        // Show staff options
    }

    // Permission checks
    if (User.CanManageEmployees())
    {
        // Show employee management
    }

    if (User.HasPermission("ViewReports"))
    {
        // Show reports
    }

    return View();
}
```

### In Razor Views/Pages

```html
@using Chapeau.Extensions

@if (User.Identity?.IsAuthenticated ?? false)
{
    <p>Welcome, @User.GetEmployeeName()!</p>
    <p>Role: @User.GetRole()</p>

    @* Role-based visibility *@
    @if (User.HasRole("Manager"))
    {
        <a href="/admin/reports">View Reports</a>
    }

    @* Permission-based visibility *@
    @if (User.CanManageEmployees())
    {
        <a href="/employees">Manage Employees</a>
    }

    @* Multiple permission check *@
    @if (User.HasAnyPermission("TakeOrders", "PrepareFood"))
    {
        <a href="/orders">Orders</a>
    }

    @* Show all user permissions (for debugging) *@
    <p>Permissions: @string.Join(", ", User.GetPermissions())</p>
}
```

## Extension Methods

Available in `Chapeau\Extensions\ClaimsPrincipalExtensions.cs`

### Basic Information
- `GetEmployeeId()` - Get employee ID (int)
- `GetEmployeeName()` - Get employee full name
- `GetUsername()` - Get username
- `GetRole()` - Get role as string
- `IsActive()` - Check if employee is active

### Role Checks
- `HasRole(role)` - Check if user has specific role
- `HasAnyRole(roles)` - User has at least one role
- `HasAllRoles(roles)` - User has all specified roles

### Permission Checks
- `HasPermission(permission)` - Check if user has specific permission
- `HasAnyPermission(permissions)` - User has at least one permission
- `HasAllPermissions(permissions)` - User has all specified permissions
- `GetPermissions()` - Get list of all user permissions

### Permission Shortcuts (convenience methods)
- `CanViewMenu()` - Check "ViewMenu" permission
- `CanTakeOrders()` - Check "TakeOrders" permission
- `CanPrepareFood()` - Check "PrepareFood" permission
- `CanManageEmployees()` - Check "ManageEmployees" permission
- `CanManageMenuItems()` - Check "ManageMenuItems" permission
- `CanViewReports()` - Check "ViewReports" permission
- `CanManageRoles()` - Check "ManageRoles" permission

## Database Schema

### Roles Table
```
RoleID (int, PK)
RoleName (varchar)
```

**Seeded Values:**
- 1 = Waiter
- 2 = Kitchen
- 3 = Manager

### Permissions Table
```
PermissionID (int, PK)
PermissionName (varchar)
Description (varchar)
```

**Seeded Values:**
- ViewMenu
- TakeOrders
- PrepareFood
- ManageEmployees
- ManageMenuItems
- ViewReports
- ManageRoles

### RolePermissions Table (Junction Table)
```
RoleID (int, FK)
PermissionID (int, FK)
```

**Role Assignments:**
- **Waiter**: ViewMenu, TakeOrders
- **Kitchen**: ViewMenu, PrepareFood
- **Manager**: All permissions

## Authorization Patterns

### 1. Attribute-Based (Recommended)
```csharp
[Authorize(Roles = "Manager")]
public IActionResult AdminPanel() => View();

[Authorize(Policy = "CanManageEmployees")]
public IActionResult ManageEmployees() => View();
```

### 2. Code-Based
```csharp
[Authorize]
public IActionResult SomeAction()
{
    if (!User.HasRole("Manager"))
        return Forbid();

    return View();
}
```

### 3. View-Based
```html
@if (User.CanManageEmployees())
{
    <div>Manager-only content</div>
}
```

## Adding Custom Claims

To add more claims, modify `ClaimsService.CreateClaims()`:

```csharp
public List<Claim> CreateClaims(Employee employee)
{
    var claims = new List<Claim>
    {
        // ... existing claims ...
        new Claim("DepartmentID", employee.DepartmentID.ToString()),
        new Claim("Shift", employee.Shift)
    };

    return claims;
}
```

Then access them with:
```csharp
var department = User.FindFirst("DepartmentID")?.Value;
var shift = User.FindFirst("Shift")?.Value;
```

## Adding Custom Authorization Policies

To add more policies, add to `Program.cs`:

```csharp
options.AddPolicy("CanApproveOrders", policy =>
    policy.RequireClaim("Permission", "ApproveOrders"));

options.AddPolicy("IsSeniorManager", policy =>
    policy.RequireRole("Manager")
    .RequireClaim("YearsExperience", claim => 
        int.Parse(claim.Value ?? "0") >= 5));
```

Then use with:
```csharp
[Authorize(Policy = "CanApproveOrders")]
public IActionResult ApproveOrders() => View();
```

## Configuration

The authentication and authorization is configured in `Program.cs`:

```csharp
// Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Authorization with policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageEmployees", policy =>
        policy.RequireClaim("Permission", "ManageEmployees"));
});
```

## Current Test Users

After inserting test data:

| Name | Role | Permissions |
|------|------|------------|
| John Waiter | Waiter | ViewMenu, TakeOrders |
| Jane Chef | Kitchen | ViewMenu, PrepareFood |
| Bob Manager | Manager | All permissions |

## Troubleshooting

### Claims Not Available
- Ensure user is authenticated with `[Authorize]`
- Check that `ClaimsService` is registered in `Program.cs`
- Verify claims are being created in `ClaimsService.CreateClaims()`

### Role Authorization Not Working
- Verify role claim is being created with correct role name
- Role names must match exactly: "Manager", "Waiter", "Kitchen"
- Use `[Authorize(Roles = "Manager")]` with correct capitalization

### Permission Claims Not Loading
- Verify `RoleRepository` is registered in `Program.cs`
- Check that `Roles` and `RolePermissions` tables are populated
- Verify permissions are correctly associated with roles in `RolePermissions` table
- Check logs for errors during permission loading

### Custom Claims Not Found
- Check that claim is being added in `ClaimsService.CreateClaims()`
- Verify claim key name matches when retrieving
- Use `User.FindFirst("ClaimKey")?.Value` to get custom claims

## Security Considerations

- **Current Setup**: No password validation (for development only)
- **Production**: Implement password hashing/verification in `AuthService`
- **HTTPS**: Always use HTTPS in production
- **Secure Cookies**: Cookies are automatically marked as Secure/HttpOnly
- **CSRF**: Anti-forgery tokens protect login/logout forms
- **Permission Loading**: Permissions are loaded at login and stored in cookies (no live updates)

## Performance Notes

- Permissions are loaded once at login and stored in claims
- No database calls are made to check permissions after login
- To refresh permissions, user must log out and log back in
- For real-time permission updates, implement custom authorization handlers
