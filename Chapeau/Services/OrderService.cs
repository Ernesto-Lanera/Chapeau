using System;
using System.Collections.Generic;
using System.Linq;
using Chapeau.Emums;
using Chapeau.Models;
using Chapeau.Repositories;

namespace Chapeau.Services
{
    /// <summary>
    /// Service layer for order management, delegating to the repository and enforcing business rules.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        /// <summary>Initializes the service with the order repository.</summary>
        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        /// <summary>Gets running orders filtered by food or drink type.</summary>
        public List<Order> GetRunningOrders(OrderType type)
        {
            return _orderRepository.GetRunningOrders(type);
        }

        /// <summary>Gets all running orders (food and drink combined, grouped by order).</summary>
        public List<Order> GetAllRunningOrders()
        {
            var food = _orderRepository.GetRunningOrders(OrderType.Food);
            var drink = _orderRepository.GetRunningOrders(OrderType.Drink);

            return food.Concat(drink)
                .GroupBy(o => o.OrderId)
                .Select(g => {
                    var order = g.First();
                    order.OrderItems = g.SelectMany(o => o.OrderItems ?? new List<OrderItem>()).ToList();
                    return order;
                })
                .ToList();
        }

        /// <summary>Calculates how long an order has been waiting.</summary>
        public TimeSpan GetWaitingTime(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);
            return DateTime.Now - order.OrderDate;
        }

        /// <summary>Gets all order items for an order.</summary>
        public List<OrderItem> GetOrderItemsByOrderId(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));
            }

            return _orderRepository.GetOrderItemsByOrderId(orderId);
        }

        /// <summary>Gets all table statuses for the table overview.</summary>
        public List<TableStatus> GetAllTableStatuses()
        {
            try
            {
                return _orderRepository.GetAllTableStatuses();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve table statuses.", ex);
            }
        }

        /// <summary>Marks a single order as served.</summary>
        public void MarkOrderAsServed(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));
            }

            try
            {
                _orderRepository.UpdateOrderStatus(orderId, OrderStatus.Served);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to mark order as served.", ex);
            }
        }

        /// <summary>Marks all ready orders at a table as served.</summary>
        public int MarkTableServed(int tableId)
        {
            if (tableId <= 0)
            {
                throw new ArgumentException("Ongeldig tafel ID.", nameof(tableId));
            }

            try
            {
                return _orderRepository.MarkReadyOrdersAsServed(tableId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to mark table as served.", ex);
            }
        }

        /// <summary>Updates the status of an order.</summary>
        public void UpdateOrderStatus(int orderId, OrderStatus status)
        {
            if (orderId <= 0)
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));

            _orderRepository.UpdateOrderStatus(orderId, status);
        }

        /// <summary>Updates the status of a single order item.</summary>
        public void UpdateOrderItemStatus(int orderItemId, OrderStatus status)
        {
            if (orderItemId <= 0)
                throw new ArgumentException("Ongeldig OrderItemId.", nameof(orderItemId));

            _orderRepository.UpdateOrderItemStatus(orderItemId, status);
        }

        /// <summary>Updates the order status based on whether all food and drink items are ready.</summary>
        public void UpdateOrderIfServed(int orderId)
        {
            List<OrderItem> foodItems = _orderRepository.GetOrderItemsByOrderId(orderId, OrderType.Food);
            List<OrderItem> drinkItems = _orderRepository.GetOrderItemsByOrderId(orderId, OrderType.Drink);

            bool foodDone = !foodItems.Any() || foodItems.All(i => i.OrderItemStatus == OrderStatus.ReadyToBeServed);
            bool drinkDone = !drinkItems.Any() || drinkItems.All(i => i.OrderItemStatus == OrderStatus.ReadyToBeServed);

            if (foodDone && drinkDone)
                UpdateOrderStatus(orderId, OrderStatus.ReadyToBeServed);
            else
                UpdateOrderStatus(orderId, OrderStatus.BeingPrepared);
        }

        /// <summary>Updates all order item statuses for a given type (food/drink) within an order.</summary>
        public void UpdateAllOrderItemStatuses(int orderId, OrderType type, OrderStatus status)
        {
            _orderRepository.UpdateAllOrderItemStatuses(orderId, type, status);
        }

        /// <summary>Updates order item statuses for a specific course within an order.</summary>
        public void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus status)
        {
            _orderRepository.UpdateCourseItemStatuses(orderId, course, status);
        }

        /// <summary>Gets finished orders from today, filtered by type.</summary>
        public List<Order> GetFinishedOrdersToday(OrderType type)
        {
            return _orderRepository.GetFinishedOrdersToday(type);
        }

        /// <summary>Creates a new order in the database.</summary>
        public void SaveOrderToDb(Order order)
        {
            _orderRepository.SaveOrder(order);
        }

        /// <summary>Gets the active order for a table including all its items.</summary>
        public Order? GetActiveOrderWithItemsByTableId(int tableId)
        {
            if (tableId <= 0)
                throw new ArgumentException("Ongeldig tafel ID.", nameof(tableId));

            return _orderRepository.GetActiveOrderWithItemsByTableId(tableId);
        }

        /// <summary>Updates an existing order's items with stock adjustment.</summary>
        public void UpdateExistingOrder(int orderId, List<OrderItem> newItems, List<OrderItem> oldItems)
        {
            if (orderId <= 0)
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));

            _orderRepository.UpdateExistingOrder(orderId, newItems, oldItems);
        }

        /// <summary>Saves a payment and updates order status to Paid.</summary>
        public void SavePayment(int orderId, int tableNumber, decimal tipAmount, string? feedback)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));
            }

            if (tableNumber <= 0)
            {
                throw new ArgumentException("Ongeldig tafel nummer.", nameof(tableNumber));
            }

            _orderRepository.SavePayment(orderId, tableNumber, tipAmount, feedback);
        }

        public Order? GetOrderById(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));
            }

            return _orderRepository.GetOrderById(orderId);
        }

        public Order? GetServedOrderByTableId(int tableId)
        {
            if (tableId <= 0)
            {
                throw new ArgumentException("Ongeldig tafel ID.", nameof(tableId));
            }

            return _orderRepository.GetServedOrderByTableId(tableId);
        }

        public List<Order> GetServedOrdersForPayment()
        {
            List<Order> orders = _orderRepository.GetOrdersByStatus(OrderStatus.Served);

            foreach (var order in orders)
            {
                order.Items = _orderRepository.GetOrderItemsByOrderId(order.OrderId);
                order.OrderItems = order.Items;
            }

            return orders;
        }
        public List<Order> GetServedOrdersByTableNumber(int tableNumber)
        {
            if (tableNumber <= 0)
                throw new ArgumentException("Ongeldig tafelnummer.", nameof(tableNumber));

            return _orderRepository.GetOrdersByTableNumber(tableNumber);
        }
    }

}