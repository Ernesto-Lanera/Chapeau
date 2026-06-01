namespace Chapeau.Emums
{
    public enum OrderStatus
    {
        Ordered,
        BeingPrepared,
        DrinksReady,
        FoodReady,
        ReadyToBeServed,
        Served,
        Paid
    }

    public enum OrderType
    {
        Food,
        Drink
    }

    public enum CourseType
    {
        Starter,
        Main,
        Dessert
    }
}
