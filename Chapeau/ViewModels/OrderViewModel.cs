using System;
using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class OrderViewModel
    {
        public int OrderID { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public TimeSpan WaitingTime { get; set; }
        public string ControllerName { get; set; } = "Kitchen";
        public OrderType OrderType { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new();
        public List<CourseGroupViewModel> CourseGroups { get; set; } = new();

        public string WaitingTimeDisplay
        {
            get
            {
                if (WaitingTime.TotalHours < 1)
                    return $"{(int)WaitingTime.TotalMinutes} m";
                else
                    return $"{(int)WaitingTime.TotalHours} hr {WaitingTime.Minutes} m";
            }
        }
    }
}