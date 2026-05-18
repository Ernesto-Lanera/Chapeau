namespace Chapeau.Constants
{
    public static class MenuCardConstants
    {
        // Menu card IDs
        public const int LunchCardId = 1;
        public const int DinnerCardId = 2;
        public const int DrinksCardId = 3;

        // Menu card names
        public const string LunchCardName = "Lunch";
        public const string DinnerCardName = "Diner";
        public const string DrinksCardName = "Dranken";

        // Oude namen die nog in andere code worden gebruikt
        public const int FoodMenuCard = LunchCardId;
        public const int FoodMenuCard2 = DinnerCardId;
        public const int DrinkMenuCard = DrinksCardId;

        // Tafels
        public const int MinTableNumber = 1;
        public const int MaxTableNumber = 10;

        // Sessieduur in uren
        public const int SessionDurationHours = 4;

        public static bool IsValidMenuCardId(int menuCardId)
        {
            return menuCardId == LunchCardId
                || menuCardId == DinnerCardId
                || menuCardId == DrinksCardId;
        }

        public static string GetMenuCardName(int menuCardId)
        {
            return menuCardId switch
            {
                LunchCardId => LunchCardName,
                DinnerCardId => DinnerCardName,
                DrinksCardId => DrinksCardName,
                _ => "Onbekend"
            };
        }
    }
}