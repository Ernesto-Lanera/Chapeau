using System;
using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class StatusButtonViewModel
    {
       public int OrderId { get; set; }
       public int? OrderItemId { get; set; }
       public OrderStatus CurrentStatus { get; set; }
       public string Scope { get; set; }
    }
}