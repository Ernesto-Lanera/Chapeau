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

            Order order = new Order { TableId = tableId, OrderDate = DateTime.Now, OrderItems = orderItems };
            return order;
        }

        public Order AddOrderItemToOrder(int MenuItemId, Order order, string MenuItemName)
        {
            if (order.OrderItems != null)
            {
                if (!order.OrderItems.Any(i => i.MenuItemId == MenuItemId))
                {
                    OrderItem orderitem = new OrderItem { MenuItemId = MenuItemId, Amount = 1, MenuItemName = MenuItemName };
                    order.OrderItems.Add(orderitem);
                }
                else
                {
                    var existingItem = order.OrderItems.First(i => i.MenuItemId == MenuItemId);
                    existingItem.Amount++;
                }
            }
             
            return order;

        }


        public Order RemoveItemFormOrder(int MenuItemId, Order order)
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

        public Order UpdateItemFormOrder (int MenuItemId, Order order, int NewAmount)
        {
            if (order.OrderItems != null)
            {
                var itemToUpdate = order.OrderItems.FirstOrDefault(i => i.MenuItemId == MenuItemId);
                if (itemToUpdate != null)
                {
                    itemToUpdate.Amount = NewAmount;
                }
            }
            return order;
        }

        public Order AddCommentoItem(int MenuItemId, Order order,String Comment)
        {
            if (order.OrderItems != null)
            {
                order.OrderItems[MenuItemId].Comment = Comment;
            }
            return order;
        }

        public void SaveOrderToDb(Order order)
        {

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

    
    }
}