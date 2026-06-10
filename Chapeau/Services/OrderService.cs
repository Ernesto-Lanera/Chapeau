using System;
using System.Collections.Generic;
using System.Linq;
using Chapeau.Emums;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.ViewModels;

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
            return food.Concat(drink).ToList();
        }


        public TimeSpan GetWaitingTime(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);
            return DateTime.Now - order.OrderDate;
        }

        public Order MakeNewOrder(int tableId)
        {
            List<OrderItem> orderItems = [];

            Order order = new Order { TableId = tableId, OrderDate = DateTime.Now, OrderItems = orderItems };
            return order;
        }

        public Order AddOrderItemToOrder(int menuItemId, Order order, string menuItemName)
        {
            var item = new OrderItem { MenuItemId = menuItemId, AmountOrdered = 1, MenuItemName = menuItemName };
            order.AddItem(item);
            return order;
        }

        public Order RemoveItemFromOrder(int menuItemId, Order order)
        {
            order.RemoveItem(menuItemId);
            return order;
        }

        public Order UpdateItemFromOrder(int menuItemId, Order order, int newAmount)
        {
            order.UpdateItemQuantity(menuItemId, newAmount);
            return order;
        }

        public Order UpdateItemComment(int menuItemId, Order order, string comment)
        {
            order.UpdateItemComment(menuItemId, comment);
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
        }

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
                throw new InvalidOperationException("Failed to mark orders as served.", ex);
            }
        }

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