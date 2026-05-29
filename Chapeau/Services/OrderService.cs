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

        public PaymentOrderViewModel GetPaymentOrderViewModel(int orderId, int tableNumber)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));
            }

            if (tableNumber <= 0)
            {
                throw new ArgumentException("Ongeldig tafel nummer.", nameof(tableNumber));
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
                throw new InvalidOperationException($"Failed to build payment view model: {ex.Message}", ex);
            }
        }

        private static PaymentOrderViewModel BuildPaymentViewModel(int orderId, int tableNumber, List<OrderItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            if (items.Count == 0)
            {
                throw new InvalidOperationException("Cannot create payment view model for empty order.");
            }

            // Group items by MenuItemId
            var groupedItems = items
                .GroupBy(i => i.MenuItemId)
                .Select(g =>
                {
                    var firstItem = g.First();
                    return new OrderItem
                    {
                        OrderItemId = firstItem.OrderItemId,
                        MenuItemId = g.Key,
                        MenuItem = firstItem.MenuItem,
                        Name = firstItem.Name ?? "Unknown Item",
                        Price = firstItem.Price,
                        VATRate = firstItem.VATRate,
                        AmountOrdered = g.Sum(x => x.AmountOrdered),
                        Comment = firstItem.Comment,
                        OrderId = orderId
                    };
                })
                .ToList();

            // Validate all items
            foreach (var item in groupedItems)
            {
                if (string.IsNullOrEmpty(item.Name))
                {
                    throw new InvalidOperationException($"Item {item.MenuItemId} has no name.");
                }

                if (item.Price < 0)
                {
                    throw new InvalidOperationException($"Invalid price for item {item.Name}: prices cannot be negative.");
                }

                if (item.VATRate < 0 || item.VATRate > 1)
                {
                    throw new InvalidOperationException($"Invalid VAT rate for item {item.Name}: VAT must be between 0 and 1.");
                }

                if (item.AmountOrdered <= 0)
                {
                    throw new InvalidOperationException($"Invalid quantity for item {item.Name}: quantity must be greater than zero.");
                }
            }
            //totaal berekenen
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