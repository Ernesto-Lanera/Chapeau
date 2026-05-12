# Role & Permission Authorization Quick Reference

## Overview

Your application uses a three-layer authorization system:
1. **Authentication** - User identity verification
2. **Role-Based Authorization** - What roles can do
3. **Permission-Based Authorization** - Fine-grained control

## Roles (Loaded from Roles Table)

| Role | RoleID | Permissions |
|------|--------|-----------|
| **Waiter** | 1 | ViewMenu, TakeOrders |
| **Kitchen** | 2 | ViewMenu, PrepareFood |
| **Manager** | 3 | All permissions |

## Permissions (Loaded from Permissions Table)

| Permission | Description |
|-----------|------------|
| **ViewMenu** | Can view menu items |
| **TakeOrders** | Can create and manage orders |
| **PrepareFood** | Can prepare food in kitchen |
| **ManageEmployees** | Can manage employee accounts |
| **ManageMenuItems** | Can add/edit/delete menu items |
| **ViewReports** | Can view financial reports |
| **ManageRoles** | Can manage roles and permissions |

## Authorization Attributes

### Role-Based
```csharp
[Authorize(Roles = "Manager")]                    // Manager only
[Authorize(Roles = "Manager,Waiter")]            // Manager or Waiter
[Authorize(Roles = "Waiter,Kitchen")]            // Waiter or Kitchen
```

### Permission-Based
```csharp
[Authorize(Policy = "CanManageEmployees")]       // Has ManageEmployees permission
[Authorize(Policy = "CanTakeOrders")]            // Has TakeOrders permission
[Authorize(Policy = "CanPrepareFood")]           // Has PrepareFood permission
```

### Policy-Based
```csharp
[Authorize(Policy = "IsManager")]                // Manager role policy
[Authorize(Policy = "IsWaiter")]                 // Waiter role policy
[Authorize(Policy = "IsKitchenStaff")]          // Kitchen role policy
```

## Code-Based Checks

### Role Checks
```csharp
User.HasRole("Manager")                          // Single role
User.HasAnyRole("Manager", "Waiter")            // At least one role
User.HasAllRoles("Manager", "Admin")            // All roles
```

### Permission Checks
```csharp
User.HasPermission("ManageEmployees")            // Single permission
User.HasAnyPermission("TakeOrders", "PrepareFood") // At least one
User.HasAllPermissions("ManageEmployees", "ViewReports") // All
```

### Permission Shortcuts
```csharp
User.CanViewMenu()                               // ViewMenu permission
User.CanTakeOrders()                             // TakeOrders permission
User.CanPrepareFood()                            // PrepareFood permission
User.CanManageEmployees()                        // ManageEmployees permission
User.CanManageMenuItems()                        // ManageMenuItems permission
User.CanViewReports()                            // ViewReports permission
User.CanManageRoles()                            // ManageRoles permission
```

### Get All Permissions
```csharp
List<string> permissions = User.GetPermissions() // Returns list of all permissions
```

## View-Based Authorization

### Razor Syntax
```html
@if (User.Identity?.IsAuthenticated ?? false)
{
    @* Role-based *@
    @if (User.HasRole("Manager"))
    {
        <p>Manager content</p>
    }

    @* Permission-based *@
    @if (User.CanManageEmployees())
    {
        <p>Employee management content</p>
    }

    @* Multiple permissions *@
    @if (User.HasAnyPermission("TakeOrders", "PrepareFood"))
    {
        <p>Order-related content</p>
    }

    @* Display all permissions *@
    <p>Permissions: @string.Join(", ", User.GetPermissions())</p>
}
```

## Complete Examples

### Manager Dashboard (Role-Based)
```csharp
[Authorize(Roles = "Manager")]
public IActionResult Dashboard()
{
    return View();  // Only managers
}
```

### Order Management (Permission-Based)
```csharp
[Authorize(Policy = "CanTakeOrders")]
public IActionResult CreateOrder()
{
    return View();  // Anyone who can take orders
}
```

### Complex Authorization (Multiple Checks)
```csharp
[Authorize]
public IActionResult EditOrder(int orderId)
{
    // Managers can always edit
    if (User.HasRole("Manager"))
        return View("EditOrder");

    // Waiters can only edit if they have permission
    if (User.HasRole("Waiter") && User.CanTakeOrders())
        return View("EditOrder");

    return Forbid();
}
```

### Dynamic Dashboard
```csharp
[Authorize]
public IActionResult MyDashboard()
{
    if (User.HasRole("Manager"))
        return RedirectToAction("AdminDashboard");

    if (User.HasRole("Kitchen"))
        return RedirectToAction("KitchenDashboard");

    return RedirectToAction("WaiterDashboard");
}
```

## User Information

```csharp
int id = User.GetEmployeeId();          // Get employee ID
string name = User.GetEmployeeName();   // Get full name
string username = User.GetUsername();   // Get username
string role = User.GetRole();           // Get role as string
bool isActive = User.IsActive();        // Check if active
List<string> perms = User.GetPermissions(); // Get all permissions
```

## Claim Types Used

| Claim | Value | Type |
|-------|-------|------|
| `ClaimTypes.NameIdentifier` | Employee ID | standard |
| `ClaimTypes.Name` | Username | standard |
| `ClaimTypes.GivenName` | Employee Name | standard |
| `ClaimTypes.Role` | Role (Manager, Waiter, Kitchen) | standard |
| `EmployeeID` | Employee ID | custom |
| `EmployeeName` | Employee Name | custom |
| `IsActive` | true/false | custom |
| `Permission` | Permission name (multiple claims) | custom |

## Database Tables

### Roles
```sql
SELECT * FROM Roles
-- RoleID | RoleName
-- 1      | Waiter
-- 2      | Kitchen
-- 3      | Manager
```

### Permissions
```sql
SELECT * FROM Permissions
-- PermissionID | PermissionName | Description
```

### RolePermissions
```sql
SELECT * FROM RolePermissions
-- RoleID | PermissionID
```

### Employees
```sql
SELECT * FROM Employees
-- EmployeeID | Name | PasswordHash | RoleID | IsActive
```

## How Permissions Are Loaded

1. User logs in
2. `AuthService` authenticates user and loads Employee object
3. Employee object has RoleID
4. `ClaimsService` creates claims
5. `RoleRepository` queries `RolePermissions` table for that RoleID
6. All permissions for that role are added as claims
7. Claims are stored in authentication cookie
8. All future permission checks happen in-memory (no database calls)

## Authorization Flow

```
User Request
    ↓
[Authorize] attribute?
    ├─ No → Allow access
    └─ Yes → Check authentication
            ↓
        Is authenticated?
            ├─ No → Redirect to login
            └─ Yes → Check authorization
                    ↓
                Role authorization specified?
                    ├─ Yes → User has role?
                    │       ├─ Yes → Allow
                    │       └─ No → Forbid
                    └─ No → Policy authorization?
                            ├─ Yes → User has permission?
                            │       ├─ Yes → Allow
                            │       └─ No → Forbid
                            └─ No → Allow
```

## Testing Permissions

Use these test users to test different permission levels:

```
Login: John Waiter
  - Role: Waiter
  - Permissions: ViewMenu, TakeOrders
  - Can: [Authorize(Roles = "Waiter")], [Authorize(Policy = "CanTakeOrders")]

Login: Jane Chef
  - Role: Kitchen
  - Permissions: ViewMenu, PrepareFood
  - Can: [Authorize(Roles = "Kitchen")], [Authorize(Policy = "CanPrepareFood")]

Login: Bob Manager
  - Role: Manager
  - Permissions: All
  - Can: [Authorize(Roles = "Manager")], [Authorize(Policy = "CanManageEmployees")], etc.
```

## Common Patterns

### Public Action
```csharp
public IActionResult PublicAction()
{
    return View();
}
```

### Authenticated Users Only
```csharp
[Authorize]
public IActionResult SecureAction()
{
    return View();
}
```

### Specific Role Only
```csharp
[Authorize(Roles = "Manager")]
public IActionResult AdminAction()
{
    return View();
}
```

### Specific Permission Only
```csharp
[Authorize(Policy = "CanManageEmployees")]
public IActionResult ManageEmployees()
{
    return View();
}
```

### Allow Specific Users
```csharp
[Authorize(Roles = "Manager,Admin")]
public IActionResult RestrictedAction()
{
    return View();
}
```

### Complex Logic
```csharp
[Authorize]
public IActionResult ComplexAction()
{
    if (!User.IsAuthenticated() && 
        !User.HasPermission("ViewReports"))
        return Forbid();

    return View();
}
```
