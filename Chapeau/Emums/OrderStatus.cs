namespace Chapeau.Emums
{
    public enum OrderStatus
    {
        Ordered,
        BeingPrepared,
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
