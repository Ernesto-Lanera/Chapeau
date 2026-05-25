using System;
using System.Collections.Generic;
using System.Linq;
using Chapeau.Emums;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.ViewModels;

namespace Chapeau.Services
{
    /// <summary>
    /// Service for order management including retrieval, status updates, and payment calculations.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public List<Order> GetRunningOrders()
        {
            try
            {
                return _orderRepository.GetRunningOrders();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve running orders.", ex);
            }
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
                throw new ArgumentException("Table ID must be greater than zero.", nameof(tableId));
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

        public void MarkOrderAsServed(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Order ID must be greater than zero.", nameof(orderId));
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

        public PaymentOrderViewModel GetPaymentOrderViewModel(int orderId, int tableNumber)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Order ID must be greater than zero.", nameof(orderId));
            }

            if (tableNumber <= 0)
            {
                throw new ArgumentException("Table number must be greater than zero.", nameof(tableNumber));
            }

            try
            {
                List<OrderItem> items = _orderRepository.GetOrderItemsByOrderId(orderId);

                if (items == null || items.Count == 0)
                {
                    throw new InvalidOperationException($"No items found for order {orderId}.");
                }

                return BuildPaymentViewModel(orderId, tableNumber, items);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to build payment view model.", ex);
            }
        }

        /// <summary>
        /// Builds a payment view model by grouping order items by MenuItemId and using domain-computed totals.
        /// </summary>
        private static PaymentOrderViewModel BuildPaymentViewModel(int orderId, int tableNumber, List<OrderItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            if (items.Count == 0)
            {
                throw new InvalidOperationException("Cannot create payment view model for empty order.");
            }

            // Group items by MenuItemId to combine duplicates and sum quantities
            var groupedItems = items
                .GroupBy(i => i.MenuItemId)
                .Select(g => new OrderItem
                {
                    OrderItemId = g.First().OrderItemId,
                    MenuItemId = g.Key,
                    Name = g.First().Name,
                    Price = g.First().Price,
                    VATRate = g.First().VATRate,
                    Amount = g.Sum(x => x.Amount),
                    Comment = g.First().Comment,
                    OrderId = orderId
                })
                .ToList();

            // Validate prices and VAT rates
            foreach (var item in groupedItems)
            {
                if (item.Price < 0)
                {
                    throw new InvalidOperationException($"Invalid price for item {item.Name}: prices cannot be negative.");
                }

                if (item.VATRate < 0 || item.VATRate > 1)
                {
                    throw new InvalidOperationException($"Invalid VAT rate for item {item.Name}: VAT must be between 0 and 100%.");
                }

                if (item.Amount <= 0)
                {
                    throw new InvalidOperationException($"Invalid quantity for item {item.Name}: quantity must be greater than zero.");
                }
            }

            // Create a temporary order to leverage domain-computed properties
            var order = new Order { Items = groupedItems };

            return new PaymentOrderViewModel
            {
                OrderID = orderId,
                TableNumber = tableNumber,
                Items = groupedItems.AsReadOnly(),
                LowVAT = Math.Round(order.LowVATTotal, 2),
                HighVAT = Math.Round(order.HighVATTotal, 2),
                Total = Math.Round(order.GrandTotal, 2)
            };
        }
    }
}