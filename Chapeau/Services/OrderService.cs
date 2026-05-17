using System;
using System.Collections.Generic;
using System.Linq;
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
            try
            {
                _orderRepository = orderRepository;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve running orders from the database: " + ex.Message);
            }
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

        public PaymentOrderViewModel GetPaymentOrderViewModel(int orderId, int tableNumber)
        {
            try
            {
                List<OrderItem> items = _orderRepository.GetOrderItemsByOrderId(orderId);

                decimal lowVat = items.Where(i => i.VATRate == 0.09m)
                                      .Sum(i => i.Price * i.AmountOrdered * i.VATRate);
                decimal highVat = items.Where(i => i.VATRate == 0.21m)
                                       .Sum(i => i.Price * i.AmountOrdered * i.VATRate);
                decimal total = items.Sum(i => i.Price * i.AmountOrdered * (1 + i.VATRate));

                return new PaymentOrderViewModel
                {
                    OrderID = orderId,
                    TableNumber = tableNumber,
                    Items = items,
                    LowVAT = lowVat,
                    HighVAT = highVat,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to build payment view model: " + ex.Message);
            }
        }
    }
}