using System;
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