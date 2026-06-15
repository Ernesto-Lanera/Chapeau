namespace Chapeau.Emums
{
    /// <summary>
    /// Tracks the lifecycle of an order from placement through payment.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>Order has been placed and is waiting to be processed.</summary>
        Ordered,
        /// <summary>Items are currently being prepared.</summary>
        BeingPrepared,
        /// <summary>Drink items are ready to be served.</summary>
        DrinksReady,
        /// <summary>Food items are ready to be served.</summary>
        FoodReady,
        /// <summary>All items are ready to be served to the table.</summary>
        ReadyToBeServed,
        /// <summary>Items have been served to the table.</summary>
        Served,
        /// <summary>Order has been paid and is complete.</summary>
        Paid
    }

    /// <summary>
    /// Categorizes order items into Food or Drink for kitchen/bar routing.
    /// </summary>
    public enum OrderType
    {
        /// <summary>Food items prepared by kitchen staff.</summary>
        Food,
        /// <summary>Drink items prepared by bar staff.</summary>
        Drink
    }

    /// <summary>
    /// Course classification for food items used for course-based preparation flow.
    /// </summary>
    public enum CourseType
    {
        /// <summary>Appetizer or starter course.</summary>
        Starter,
        /// <summary>Main course.</summary>
        Main,
        /// <summary>Dessert course.</summary>
        Dessert
    }
}
