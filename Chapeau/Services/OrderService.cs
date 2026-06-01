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
    }
}