namespace Chapeau.Constants
{
    /// Centralized error messages for consistent error handling across the application.
    public static class ErrorMessages
    {
        // Connection & Database
        public const string ConnectionStringMissing = "Database-verbindingsstring ontbreekt.";

        // Menu Items
        public const string RetrieveMenuItemsError = "Er is een fout opgetreden bij het ophalen van menu-items.";
        public const string AddMenuItemError = "Er is een fout opgetreden bij het toevoegen van het menu-item.";
        public const string UpdateMenuItemError = "Er is een fout opgetreden bij het bijwerken van het menu-item.";
        public const string MenuItemNotFound = "Menu-item niet gevonden.";
        public const string MenuItemDuplicateName = "Een menu-item met deze naam bestaat al.";
        public const string UpdateMenuItemAlreadyExists = "Een ander menu-item met deze naam bestaat al.";

        // Employees
        public const string EmployeeNotFound = "Medewerker niet gevonden.";
        public const string UsernameTaken = "Deze gebruikersnaam is al in gebruik. Probeer een ander.";
        public const string UpdateEmployeeActiveError = "Er is een fout opgetreden bij het bijwerken van de actieve status van de medewerker.";

        // Categories
        public const string RetrieveCategoriesError = "Er is een fout opgetreden bij het ophalen van categorieën.";
        public const string AddCategoryError = "Er is een fout opgetreden bij het toevoegen van de categorie.";
        public const string UpdateCategoryError = "Er is een fout opgetreden bij het bijwerken van de categorie.";
        public const string CategoryNotFound = "Categorie niet gevonden.";

        // Roles
        public const string RoleNotFound = "Rol niet gevonden.";

        // Status
        public const string RetrieveStatusError = "Er is een fout opgetreden bij het ophalen van statussen.";

        // General
        public const string UnexpectedError = "Er is een onverwachte fout opgetreden. Probeer het later opnieuw.";
    }
}