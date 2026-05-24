namespace Chapeau.Constants.Login
{
    public static class AuthConstants
    {
        public const int SessionDurationHours = 8;
        public const string LoginPath = "/Account/Login";

        public const string InvalidCredentialsError = "Ongeldige naam of wachtwoord.";
        public const string InactiveAccountError = "Je account is inactief. Neem contact op met de manager.";
        public const string DatabaseConnected = "Verbonden met de database.";
        public const string DatabaseNotConnectedPrefix = "Niet verbonden met de database: ";
    }
}
