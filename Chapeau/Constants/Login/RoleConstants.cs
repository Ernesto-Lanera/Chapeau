namespace Chapeau.Constants.Login
{
    /// <summary>
    /// Defines the fixed role identifiers and display names used throughout the application.
    /// These map to the Roles database table and are referenced by authorization policies and dashboard routing.
    /// </summary>
    public static class RoleConstants
    {
        /// <summary>Database ID for the Manager role.</summary>
        public const int ManagerId = 1;
        /// <summary>Database ID for the Bediening (waiter/service staff) role.</summary>
        public const int BedieningId = 3;
        /// <summary>Database ID for the Keuken (kitchen staff) role.</summary>
        public const int KeukenId = 4;
        /// <summary>Database ID for the Barman (bartender) role.</summary>
        public const int BarmanId = 5;

        /// <summary>Display name for the Manager role.</summary>
        public const string ManagerName = "Manager";
        /// <summary>Display name for the Bediening (waiter) role.</summary>
        public const string BedieningName = "Bediening";
        /// <summary>Display name for the Keuken (kitchen) role.</summary>
        public const string KeukenName = "Keuken";
        /// <summary>Display name for the Barman (bartender) role.</summary>
        public const string BarmanName = "Barman";
        /// <summary>Fallback display name when a role ID does not match any known role.</summary>
        public const string UnknownName = "Onbekend";
    }
}
