using System;
using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class CourseGroupViewModel
    {
       public CourseType Course { get; set; }
       public List<OrderItemViewModel> Items { get; set; } = new();
    }
}