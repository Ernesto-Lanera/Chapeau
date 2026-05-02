namespace Chapeau.Constants
{
    /// Centralized error messages for consistent error handling across the application.
    public static class ErrorMessages
    {
        // Connection & Database
        public const string ConnectionStringMissing = "Database connection string is missing.";
        public const string FailedToConnectDatabase = "Failed to connect to database.";

        // Menu Items
        public const string RetrieveMenuItemsError = "An error occurred while retrieving menu items.";
        public const string AddMenuItemError = "An error occurred while adding the menu item.";
        public const string UpdateMenuItemError = "An error occurred while updating the menu item.";
        public const string MenuItemNotFound = "Menu item not found.";
        public const string MenuItemDuplicateName = "A menu item with this name already exists.";
        public const string UpdateMenuItemAlreadyExists = "Another menu item with this name already exists.";

        // Employees
        public const string RetrieveEmployeesError = "An error occurred while retrieving employees.";
        public const string AddEmployeeError = "An error occurred while adding the employee.";
        public const string UpdateEmployeeError = "An error occurred while updating the employee.";
        public const string EmployeeNotFound = "Employee not found.";
        public const string UsernameTaken = "This username is already taken. Please try another one.";
        public const string UsernameAlreadyTaken = "This username is already taken by another employee.";
        public const string UpdateEmployeeActiveError = "An error occurred while updating the employee's active status.";

        // Categories
        public const string RetrieveCategoriesError = "An error occurred while retrieving categories.";

        // General
        public const string UnexpectedError = "An unexpected error occurred. Please try again later.";
    }
}