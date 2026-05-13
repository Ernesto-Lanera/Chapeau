namespace Chapeau.Constants
{
    /// Centralized error messages for consistent error handling across the application.
    public static class ErrorMessages
    {
        // Connection & Database
        public const string ConnectionStringMissing = "Database-verbindingsstring ontbreekt.";
        public const string FailedToConnectDatabase = "Verbinding met database mislukt.";

        // Menu Items
        public const string RetrieveMenuItemsError = "Er is een fout opgetreden bij het ophalen van menu-items.";
        public const string AddMenuItemError = "Er is een fout opgetreden bij het toevoegen van het menu-item.";
        public const string UpdateMenuItemError = "Er is een fout opgetreden bij het bijwerken van het menu-item.";
        public const string MenuItemNotFound = "Menu-item niet gevonden.";
        public const string MenuItemDuplicateName = "Een menu-item met deze naam bestaat al.";
        public const string UpdateMenuItemAlreadyExists = "Een ander menu-item met deze naam bestaat al.";

        // Employees
        public const string RetrieveEmployeesError = "Er is een fout opgetreden bij het ophalen van medewerkers.";
        public const string AddEmployeeError = "Er is een fout opgetreden bij het toevoegen van de medewerker.";
        public const string UpdateEmployeeError = "Er is een fout opgetreden bij het bijwerken van de medewerker.";
        public const string EmployeeNotFound = "Medewerker niet gevonden.";
        public const string UsernameTaken = "Deze gebruikersnaam is al in gebruik. Probeer een ander.";
        public const string UsernameAlreadyTaken = "Deze gebruikersnaam wordt al gebruikt door een andere medewerker.";
        public const string UpdateEmployeeActiveError = "Er is een fout opgetreden bij het bijwerken van de actieve status van de medewerker.";

        // Categories
        public const string RetrieveCategoriesError = "Er is een fout opgetreden bij het ophalen van categorieën.";

        // General
        public const string UnexpectedError = "Er is een onverwachte fout opgetreden. Probeer het later opnieuw.";
    }
}