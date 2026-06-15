namespace Chapeau.Constants.Login
{
    /// <summary>
    /// Central configuration constants for authentication behavior and error messages.
    /// </summary>
    public static class AuthConstants
    {
        /// <summary>Maximum session duration in hours before the authentication cookie expires.</summary>
        public const int SessionDurationHours = 8;
        /// <summary>Path to the login page.</summary>
        public const string LoginPath = "/Account/Login";

        /// <summary>Error message shown when the username or password is incorrect.</summary>
        public const string InvalidCredentialsError = "Ongeldige naam of wachtwoord.";
        /// <summary>Error message shown when the employee account has been deactivated.</summary>
        public const string InactiveAccountError = "Je account is inactief. Neem contact op met de manager.";
        /// <summary>Message indicating a successful database connection.</summary>
        public const string DatabaseConnected = "Verbonden met de database.";
        /// <summary>Prefix for database connection failure messages.</summary>
        public const string DatabaseNotConnectedPrefix = "Niet verbonden met de database: ";
    }
}
