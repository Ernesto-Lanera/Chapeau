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
            return _orderRepository.GetRunningOrders(type);
        }

       

        public TimeSpan GetWaitingTime(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);
            return DateTime.Now - order.OrderDate;
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

        public void UpdateOrderStatus(int orderId, OrderStatus status)
        {
            if (orderId <= 0)
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));

            _orderRepository.UpdateOrderStatus(orderId, status);
        }

        public void UpdateOrderItemStatus(int orderItemId, OrderStatus status)
        {
            if (orderItemId <= 0)
                throw new ArgumentException("Ongeldig OrderItemId.", nameof(orderItemId));

            _orderRepository.UpdateOrderItemStatus(orderItemId, status);
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
            _orderRepository.UpdateAllOrderItemStatuses(orderId, type, status);
        }

        public void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus status)
        {
            _orderRepository.UpdateCourseItemStatuses(orderId, course, status);
        }

        public List<Order> GetFinishedOrdersToday(OrderType type)
        {
            return _orderRepository.GetFinishedOrdersToday(type);
        }

        public void SaveOrderToDb(Order order)
        {
            _orderRepository.SaveOrder(order);
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
        public List<Order> GetServedOrdersByTableId(int tableId)
        {
            if (tableId <= 0)
                throw new ArgumentException("Ongeldig tafel ID.", nameof(tableId));

            return _orderRepository.GetOrdersByTableId(tableId);
        }
    }

}