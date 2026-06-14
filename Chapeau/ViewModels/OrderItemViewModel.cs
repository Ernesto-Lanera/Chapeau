using System;
using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class OrderItemViewModel
    {
        public int OrderItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string? Comment { get; set; }
        public OrderStatus Status { get; set; }

        public bool IsReady => Status == OrderStatus.ReadyToBeServed;
        public bool IsPreparing => Status == OrderStatus.BeingPrepared;

        public int NextStatus => IsPreparing
            ? (int)OrderStatus.ReadyToBeServed
            : (int)OrderStatus.BeingPrepared;

        public string ButtonCssClass => IsPreparing ? "btn-warning" : "btn-outline-secondary";
        public string ButtonLabel => IsPreparing ? "Ready" : "Prepare";
    }
}