using System;
using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class OrderItemViewModel
    {
        public int OrderItemId { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public string? Comment { get; set; }
        public OrderStatus Status { get; set; }
        public CourseType? CourseType { get; set; }
    }
}