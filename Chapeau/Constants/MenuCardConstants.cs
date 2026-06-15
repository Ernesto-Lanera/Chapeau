namespace Chapeau.Constants
{
    /// <summary>
    /// Central constants for menu card definitions, table ranges, and session configuration.
    /// </summary>
    public static class MenuCardConstants
    {
        /// <summary>Database ID for the Lunch menu card.</summary>
        public const int LunchCardId = 1;
        /// <summary>Database ID for the Dinner menu card.</summary>
        public const int DinnerCardId = 2;
        /// <summary>Database ID for the Drinks menu card.</summary>
        public const int DrinksCardId = 3;

        /// <summary>Display name for the Lunch menu card.</summary>
        public const string LunchCardName = "Lunch";
        /// <summary>Display name for the Dinner menu card.</summary>
        public const string DinnerCardName = "Diner";
        /// <summary>Display name for the Drinks menu card.</summary>
        public const string DrinksCardName = "Dranken";

        /// <summary>Alias for LunchCardId used in food/drink filtering queries.</summary>
        public const int FoodMenuCard = LunchCardId;
        /// <summary>Alias for DinnerCardId used in food/drink filtering queries.</summary>
        public const int FoodMenuCard2 = DinnerCardId;
        /// <summary>Alias for DrinksCardId used in food/drink filtering queries.</summary>
        public const int DrinkMenuCard = DrinksCardId;

        /// <summary>Lowest table number in the restaurant.</summary>
        public const int MinTableNumber = 1;
        /// <summary>Highest table number in the restaurant.</summary>
        public const int MaxTableNumber = 10;

        /// <summary>Session duration in hours for admin sessions.</summary>
        public const int SessionDurationHours = 4;

        /// <summary>Returns true if the given ID matches a known menu card.</summary>
        public static bool IsValidMenuCardId(int menuCardId)
        {
            return menuCardId == LunchCardId
                || menuCardId == DinnerCardId
                || menuCardId == DrinksCardId;
        }

        /// <summary>Returns the display name for a menu card ID.</summary>
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