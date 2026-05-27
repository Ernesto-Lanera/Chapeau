namespace Chapeau.Constants
{
    public static class ErrorMessages
    {
        public const string ConnectionStringMissing = "Database-verbindingsstring ontbreekt.";

        public const string RetrieveMenuItemsError = "Er is een fout opgetreden bij het ophalen van menu-items.";
        public const string AddMenuItemError = "Er is een fout opgetreden bij het toevoegen van het menu-item.";
        public const string UpdateMenuItemError = "Er is een fout opgetreden bij het bijwerken van het menu-item.";
        public const string MenuItemNotFound = "Menu-item niet gevonden.";
        public const string InactiveMenuItemStockChangeNotAllowed = "De voorraad van een inactief menu-item kan niet worden gewijzigd.";
        public const string MenuItemDuplicateName = "Een menu-item met deze naam bestaat al.";
        public const string UpdateMenuItemAlreadyExists = "Een ander menu-item met deze naam bestaat al.";
        public const string MenuItemNameRequired = "Naam is verplicht.";
        public const string MenuItemNameTooLong = "Naam mag niet langer zijn dan 100 karakters.";
        public const string MenuItemStockNegative = "Voorraad mag niet negatief zijn.";
        public const string MenuItemPriceRequired = "Prijs is verplicht.";
        public const string MenuItemPriceInvalid = "Prijs moet hoger zijn dan 0.";

        public const string InvalidMenuCard = "Kies een geldige kaart.";
        public const string InvalidCategory = "Kies een geldige categorie.";
        public const string CategoryNotBelongsToCard = "Deze categorie hoort niet bij de gekozen kaart.";
        public const string CanOnlyChangeCategoryWithinCard = "Je mag alleen een categorie kiezen binnen dezelfde kaart.";

        public const string EmployeeNotFound = "Medewerker niet gevonden.";
        public const string UsernameTaken = "Deze naam is al in gebruik. Probeer een andere.";
        public const string UpdateEmployeeActiveError = "Er is een fout opgetreden bij het bijwerken van de actieve status van de medewerker.";
        public const string EmployeeNameRequired = "Naam is verplicht.";
        public const string EmployeeNameTooLong = "Naam mag niet langer zijn dan 100 karakters.";
        public const string EmployeePasswordRequired = "Wachtwoord/Pincode is verplicht.";
        public const string EmployeeRoleRequired = "Kies een geldige rol.";

        public const string RetrieveCategoriesError = "Er is een fout opgetreden bij het ophalen van categorieën.";
        public const string AddCategoryError = "Er is een fout opgetreden bij het toevoegen van de categorie.";
        public const string UpdateCategoryError = "Er is een fout opgetreden bij het bijwerken van de categorie.";
        public const string CategoryNotFound = "Categorie niet gevonden.";

        public const string RoleNotFound = "Rol niet gevonden.";

        public const string RetrieveStatusError = "Er is een fout opgetreden bij het ophalen van statussen.";

        public const string UnexpectedError = "Er is een onverwachte fout opgetreden. Probeer het later opnieuw.";
        public const string ImageUploadError = "Fout bij upload van afbeelding.";
    }
}