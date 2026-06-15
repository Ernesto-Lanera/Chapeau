using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.Services
{
    /// <summary>
    /// Service interface for order management operations.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>Gets the active (Ordered or BeingPrepared) order for a table.</summary>
        Order? GetActiveOrderByTableId(int tableId);
        /// <summary>Gets the served order for a table.</summary>
        Order? GetServedOrderByTableId(int tableId);
     
        List<OrderItem> GetOrderItemsByOrderId(int orderId);
        /// <summary>Gets running orders filtered by food or drink type.</summary>
        List<Order> GetRunningOrders(OrderType type);
        /// <summary>Gets all running orders (food and drink combined).</summary>
        List<Order> GetAllRunningOrders();
        /// <summary>Gets all served orders ready for payment.</summary>
        List<Order> GetServedOrdersForPayment();
        /// <summary>Calculates how long an order has been waiting.</summary>
        TimeSpan GetWaitingTime(Order order);
        /// <summary>Gets all table statuses for the table overview.</summary>
        List<TableStatus> GetAllTableStatuses();
        /// <summary>Updates the status of an order.</summary>
        void UpdateOrderStatus(int orderId, OrderStatus status);
        /// <summary>Updates the status of a single order item.</summary>
        void UpdateOrderItemStatus(int orderItemId, OrderStatus status);
        /// <summary>Marks a single order as served.</summary>
        void MarkOrderAsServed(int orderId);
        /// <summary>Marks all ready orders at a table as served.</summary>
        int MarkTableServed(int tableId);
        /// <summary>Saves a payment and updates order status to Paid.</summary>
        void SavePayment(int orderId, int tableNumber, decimal tipAmount, string? feedback);
        /// <summary>Updates the order status based on food/drink readiness.</summary>
        void UpdateOrderIfServed(int orderId);
        /// <summary>Updates all order item statuses for a given type within an order.</summary>
        void UpdateAllOrderItemStatuses(int orderId, OrderType type, OrderStatus status);
        /// <summary>Updates order item statuses for a specific course within an order.</summary>
        void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus status);
        /// <summary>Gets finished orders from today, filtered by type.</summary>
        List<Order> GetFinishedOrdersToday(OrderType type);
        /// <summary>Creates a new empty order for a table.</summary>
        Order MakeNewOrder(int tableId);
        /// <summary>Adds an item to an in-memory order.</summary>
        Order AddOrderItemToOrder(int menuItemId, Order order, string menuItemName);
        /// <summary>Removes an item from an in-memory order.</summary>
        Order RemoveItemFromOrder(int menuItemId, Order order);
        /// <summary>Updates item quantity in an in-memory order.</summary>
        Order UpdateItemFromOrder(int menuItemId, Order order, int newAmount);
        /// <summary>Changes the comment on an in-memory order item.</summary>
        Order ChangeCommentInItem(int menuItemId, Order order, string comment);
        /// <summary>Updates a comment on an in-memory order item.</summary>
        Order UpdateItemComment(int menuItemId, Order order, string comment);
        /// <summary>Updates a comment on a persisted order item.</summary>
        void UpdateItemComment(int orderItemId, string comment);
        /// <summary>Creates a new order in the database.</summary>
        void SaveOrderToDb(Order order);
        /// <summary>Gets the active order for a table including all items.</summary>
        Order? GetActiveOrderWithItemsByTableId(int tableId);
        /// <summary>Updates an existing order's items with stock adjustment.</summary>
        void UpdateExistingOrder(int orderId, List<OrderItem> newItems, List<OrderItem> oldItems);
        /// <summary>Gets served orders by table number.</summary>
        List<Order> GetServedOrdersByTableNumber(int tableNumber);
    }
}
