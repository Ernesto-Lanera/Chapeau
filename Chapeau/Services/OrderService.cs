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

        public List<Order> GetRunningOrders()
        {
            try
            {
                return _orderRepository.GetRunningOrders();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve running orders: " + ex.Message);
            }
        }

        public TimeSpan GetWaitingTime(Order order)
        {
            return DateTime.Now - order.OrderDate;
        }

        public Order MakeNewOrder(int tableId)
        {
            Dictionary<int, OrderItem> orderItems = [];

            Order order = new Order { TableId = tableId, OrderDate = DateTime.Now, OrderItems = orderItems };
            return order;
        }

        public Order AddMenuItemToOrder(MenuItem MenuItem, Order order)
        {
            if (order.OrderItems != null)
            {
                if (!order.OrderItems.ContainsKey(MenuItem.MenuItemID))
                {
                    OrderItem orderitem = new OrderItem { MenuItemId = MenuItem.MenuItemID, Amount = 1, MenuItemName = MenuItem.Name };
                    order.OrderItems.Add(MenuItem.MenuItemID, orderitem);
                }
                else
                {
                    order.OrderItems[MenuItem.MenuItemID].Amount++;
                }
            }
             
            return order;

        }

        public void SaveOrderToDb(Order order)
        {

        }

        public Order GetActiveOrderByTableId(int tableId)
        {
            try
            {
                return _orderRepository.GetActiveOrderByTableId(tableId);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve order for table: " + ex.Message);
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
                throw new Exception("Failed to retrieve table statuses: " + ex.Message);
            }
        }

        public void MarkOrderAsServed(int orderId)
        {
            try
            {
                _orderRepository.UpdateOrderStatus(orderId, OrderStatus.Served);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to mark order as served: " + ex.Message);
            }
        }

        public PaymentOrderViewModel GetPaymentOrderViewModel(int orderId, int tableNumber)
        {
            try
            {
                List<OrderItem> items = _orderRepository.GetOrderItemsByOrderId(orderId);
                return BuildPaymentViewModel(orderId, tableNumber, items);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to build payment view model: " + ex.Message);
            }
        }

        private static PaymentOrderViewModel BuildPaymentViewModel(int orderId, int tableNumber, List<OrderItem> items)
        {
            // Group items by name to combine duplicates and sum quantities
            var groupedItems = items.GroupBy(i => i.MenuItemId)
                                   .Select(g => new OrderItem
                                   {
                                       OrderItemId = g.First().OrderItemId,
                                       MenuItemId = g.Key,
                                       Name = g.First().Name,
                                       Price = g.First().Price,
                                       VATRate = g.First().VATRate,
                                       AmountOrdered = g.Sum(x => x.AmountOrdered),
                                       Comment = g.First().Comment,
                                       OrderId = orderId
                                   })
                                   .ToList();

            decimal lowVat = groupedItems.Where(i => i.VATRate == 0.06m)
                                        .Sum(i => i.Price * i.AmountOrdered * i.VATRate);
            decimal highVat = groupedItems.Where(i => i.VATRate == 0.21m)
                                         .Sum(i => i.Price * i.AmountOrdered * i.VATRate);
            decimal total = groupedItems.Sum(i => i.Price * i.AmountOrdered * (1 + i.VATRate));

            return new PaymentOrderViewModel
            {
                OrderID = orderId,
                TableNumber = tableNumber,
                Items = groupedItems,
                LowVAT = lowVat,
                HighVAT = highVat,
                Total = total
            };
        }
    }
}