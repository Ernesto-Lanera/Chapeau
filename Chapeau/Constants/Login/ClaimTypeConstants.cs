namespace Chapeau.Constants.Login
{
    /// <summary>
    /// Custom claim type identifiers used for authentication and authorization.
    /// These extend the standard <see cref="System.Security.Claims.ClaimTypes"/> with application-specific claims.
    /// </summary>
    public static class ClaimTypeConstants
    {
        /// <summary>Claim type for the employee's unique identifier.</summary>
        public const string EmployeeId = "EmployeeID";
        /// <summary>Claim type for the employee's display name.</summary>
        public const string EmployeeName = "EmployeeName";
        /// <summary>Claim type for the employee's role identifier.</summary>
        public const string RoleId = "RoleID";
        /// <summary>Claim type for the employee's role display name.</summary>
        public const string RoleName = "RoleName";
        /// <summary>Claim type indicating whether the employee account is active.</summary>
        public const string IsActive = "IsActive";
        /// <summary>Claim type for individual permission values.</summary>
        public const string Permission = "Permission";
    }
}
