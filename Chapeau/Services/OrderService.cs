using System;
using System.Collections.Generic;
using System.Linq;
using Chapeau.Emums;
using Chapeau.Models;
using Chapeau.Repositories;

namespace Chapeau.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public List<Order> GetRunningOrders(OrderType type)
        {
            try
            {
                return _orderRepository.GetRunningOrders(type);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve running orders.", ex);
            }
        }

        public List<Order> GetAllRunningOrders()
        {
            var food = _orderRepository.GetRunningOrders(OrderType.Food);
            var drink = _orderRepository.GetRunningOrders(OrderType.Drink);

            return food.Concat(drink)
                .GroupBy(o => o.OrderId)
                .Select(g => {
                    var order = g.First();
                    order.Items = g.SelectMany(o => o.Items ?? new List<OrderItem>()).ToList();
                    return order;
                })
                .ToList();
        }

        public TimeSpan GetWaitingTime(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);
            return DateTime.Now - order.OrderDate;
        }

        public Order MakeNewOrder(int tableId)
        {
            List<OrderItem> orderItems = [];

            Order order = new Order { TableId = tableId, OrderDate = DateTime.Now, OrderItems = orderItems, GuestName = "bob"};
            return order;
        }

        public Order AddOrderItemToOrder(int MenuItemId, Order order, string MenuItemName)
        {
            if (order.OrderItems != null)
            {
                if (!order.OrderItems.Any(i => i.MenuItemId == MenuItemId))
                {
                    OrderItem orderitem = new OrderItem { MenuItemId = MenuItemId, AmountOrdered = 1, MenuItemName = MenuItemName };
                    order.OrderItems.Add(orderitem);
                }
                else
                {
                    var existingItem = order.OrderItems.First(i => i.MenuItemId == MenuItemId);
                    existingItem.AmountOrdered++;
                }
            }
             
            return order;

        }


        public Order RemoveItemFromOrder(int MenuItemId, Order order)
        {
            if (order.OrderItems != null)
            {
                var itemToRemove = order.OrderItems.FirstOrDefault(i => i.MenuItemId == MenuItemId);
                if (itemToRemove != null)
                {
                    order.OrderItems.Remove(itemToRemove);
                }
            }
            return order;
        }

        public List<OrderItem> GetOrderItemsByOrderId(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));
            }

            return _orderRepository.GetOrderItemsByOrderId(orderId);
        }

        public List<TableStatus> GetAllTableStatuses()
        public Order UpdateItemFromOrder (int MenuItemId, Order order, int NewAmount)
        {
            if (order.OrderItems != null)
            {
                var itemToUpdate = order.OrderItems.FirstOrDefault(i => i.MenuItemId == MenuItemId);
                if (itemToUpdate != null)
                {
                    itemToUpdate.AmountOrdered = NewAmount;
                }
            }
            return order;
        }

        public Order ChangeCommentinItem(int MenuItemId, Order order,String Comment)
        {
            if (order.OrderItems != null)
            {
                var itemToComment = order.OrderItems.FirstOrDefault(i => i.MenuItemId == MenuItemId);
                if (itemToComment != null && String.IsNullOrEmpty(Comment))
                {
                    itemToComment.Comment = Comment;
                }
                else
                {
                    itemToComment?.Comment = null;
                }
 
            }
            return order;
        }

        public void UpdateOrderStatus(int orderId, OrderStatus status)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));
            }

            try
            {
                _orderRepository.UpdateOrderStatus(orderId, status);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to mark order as served.", ex);
            }
        }

        public void SavePayment(int orderId, int tableNumber, decimal tipAmount, string? feedback)
        public void UpdateOrderItemStatus(int orderItemId, OrderStatus status)
        {
            if (orderItemId <= 0)
            {
                throw new ArgumentException("Ongeldig OrderItemId.", nameof(orderItemId));
            }

            try
            {
                _orderRepository.UpdateOrderItemStatus(orderItemId, status);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to mark order as served.", ex);
            }
        }

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
        public void UpdateAllOrderItemStatuses(int orderId, OrderType type, OrderStatus status)
        {
            try
            {
                _orderRepository.UpdateAllOrderItemStatuses(orderId, type, status);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update all order item statuses.", ex);
            }
        }

        public void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus status)
        {
            try
            {
                _orderRepository.UpdateCourseItemStatuses(orderId, course, status);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update course item statuses.", ex);
            }
        }

        public List<Order> GetFinishedOrdersToday(OrderType type)
        {
            try
            {
                return _orderRepository.GetFinishedOrdersToday(type);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve finished orders.", ex);
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
        public void SaveOrderToDb(Order order)
        {
            _orderRepository.SaveOrder(order);
        }

        public Order GetActiveOrderByTableId(int tableId)
        {
            if (tableId <= 0)
            {
                throw new ArgumentException("Ongeldig tafel ID.", nameof(tableId));
            }

            try
            {
                return _orderRepository.GetActiveOrderByTableId(tableId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve order for table.", ex);
            }
        }

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

            return orders;
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

    
    }
}