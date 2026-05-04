namespace Chapeau.Constants
{
    /// Database column constraints and field length limitations.
    /// Keep these in sync with the database schema.
    public static class DatabaseConstraints
    {
        // Employee constraints
        public const int EmployeeNameMaxLength = 100;
        public const int EmployeeUsernameMaxLength = 50;
        public const int EmployeePasswordHashMaxLength = 255;
        public const int EmployeeRoleMaxLength = 50;

        // MenuItem constraints
        public const int MenuItemNameMaxLength = 100;

        // Category constraints
        public const int CategoryNameMaxLength = 100;
    }
}