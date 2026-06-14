using System;
using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class CourseGroupViewModel
    {
       public CourseType Course { get; set; }
       public List<OrderItemViewModel> Items { get; set; } = new();
       public bool AllReady => Items.All(i => i.Status == OrderStatus.ReadyToBeServed);
       public bool AnyPreparing => Items.Any(i => i.Status == OrderStatus.BeingPrepared);
    }
}