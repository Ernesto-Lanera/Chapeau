using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.Repositories
{
    /// <summary>
    /// Repository interface for order and order item data access.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>Gets running (non-finished) orders filtered by food or drink type.</summary>
        List<Order> GetRunningOrders(OrderType type);
        /// <summary>Gets order items for an order, filtered by food or drink type.</summary>
        List<OrderItem> GetOrderItemsByOrderId(int orderId, OrderType type);
        /// <summary>Gets all order items for an order.</summary>
        List<OrderItem> GetOrderItemsByOrderId(int orderId);
        /// <summary>Gets the active (Ordered or BeingPrepared) order for a table, without items.</summary>
        Order? GetActiveOrderByTableId(int tableId);
        /// <summary>Gets the served order for a table, without items.</summary>
        Order? GetServedOrderByTableId(int tableId);
        /// <summary>Gets an order by its ID, without items.</summary>
        Order? GetOrderById(int orderId);
        /// <summary>Gets all orders matching a given status.</summary>
        List<Order> GetOrdersByStatus(OrderStatus status);
        /// <summary>Gets all table statuses with their active orders for the table overview.</summary>
        List<TableStatus> GetAllTableStatuses();
        /// <summary>Updates the status of an order.</summary>
        void UpdateOrderStatus(int orderId, OrderStatus newStatus);
        /// <summary>Updates the status of a single order item.</summary>
        void UpdateOrderItemStatus(int orderItemId, OrderStatus newStatus);
        /// <summary>Updates the comment on a single order item.</summary>
        void UpdateItemComment(int orderItemId, string? comment);
        /// <summary>Saves a payment record and updates the order/table status.</summary>
        void SavePayment(int orderId, int tableNumber, decimal tipAmount, string? feedback);
        /// <summary>Updates all order item statuses for a given type (food/drink) in an order.</summary>
        void UpdateAllOrderItemStatuses(int orderId, OrderType type, OrderStatus newStatus);
        /// <summary>Updates order item statuses for a specific course within an order.</summary>
        void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus newStatus);
        /// <summary>Gets finished orders (ReadyToBeServed/Served/Paid) from today, filtered by type.</summary>
        List<Order> GetFinishedOrdersToday(OrderType type);
        /// <summary>Marks all ReadyToBeServed orders at a table as Served. Returns the number updated.</summary>
        int MarkReadyOrdersAsServed(int tableId);
        /// <summary>Creates a new order with its items in the database.</summary>
        void SaveOrder(Order order);
        /// <summary>Gets orders for a specific table number in Served status.</summary>
        List<Order> GetOrdersByTableNumber(int tableNumber);
        /// <summary>Gets the active order for a table including all its items.</summary>
        Order? GetActiveOrderWithItemsByTableId(int tableId);
        /// <summary>Updates an existing order by replacing all items and adjusting stock atomically.</summary>
        void UpdateExistingOrder(int orderId, List<OrderItem> items, List<OrderItem> oldItems);
    }
}
