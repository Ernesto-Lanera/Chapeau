namespace Chapeau.Constants.Login
{
    /// <summary>
    /// Defines all permission names used by the authorization system.
    /// Permissions are stored in the database and assigned to roles for fine-grained access control.
    /// </summary>
    public static class PermissionConstants
    {
        /// <summary>Allows taking and managing customer orders.</summary>
        public const string TakeOrders = "TakeOrders";
        /// <summary>Allows preparing food items in the kitchen.</summary>
        public const string PrepareFood = "PrepareFood";
        /// <summary>Allows preparing drink items at the bar.</summary>
        public const string PrepareDrinks = "PrepareDrinks";
        /// <summary>Allows managing employee accounts.</summary>
        public const string ManageEmployees = "ManageEmployees";
        /// <summary>Allows managing menu items and categories.</summary>
        public const string ManageMenuItems = "ManageMenuItems";
        /// <summary>Allows managing stock levels.</summary>
        public const string ManageStock = "ManageStock";
        /// <summary>Allows viewing financial reports and summaries.</summary>
        public const string ViewFinance = "ViewFinance";
        /// <summary>Legacy permission name for backward compatibility with ViewFinance.</summary>
        public const string LegacyViewReports = "ViewReports";
        /// <summary>Allows managing roles and their permission assignments.</summary>
        public const string ManageRoles = "ManageRoles";
    }
}
