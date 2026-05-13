using Chapeau.Models;
using Chapeau.Repositories;
using System;
using System.Collections.Generic;

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
    }
}