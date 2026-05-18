namespace Chapeau.Constants
{
    public static class MenuCardConstants
    {
        public const int LunchCardId = 1;
        public const int DinnerCardId = 2;
        public const int DrinksCardId = 3;

        public const string LunchCardName = "Lunch";
        public const string DinnerCardName = "Diner";
        public const string DrinksCardName = "Dranken";

        public static string GetMenuCardName(int menuCardId) => menuCardId switch
        {
            LunchCardId => LunchCardName,
            DinnerCardId => DinnerCardName,
            DrinksCardId => DrinksCardName,
            _ => "Onbekend"
        };

        public static bool IsValidMenuCardId(int menuCardId) =>
            menuCardId is LunchCardId or DinnerCardId or DrinksCardId;

        public static IReadOnlyList<int> ValidCardIds =>
            new[] { LunchCardId, DinnerCardId, DrinksCardId };
    }
}