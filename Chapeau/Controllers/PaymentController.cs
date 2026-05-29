using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanTakeOrders")]
    public class PaymentController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;

        public PaymentController(IOrderService orderService, IConfiguration configuration)
        {
            _orderService = orderService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            List<Order> orders = _orderService.GetAllRunningOrders();
            return View(orders);
        }

        public IActionResult ViewOrder(int tableId)
        {
            try
            {
                Order? order = _orderService.GetActiveOrderByTableId(tableId);
                if (order == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                PaymentOrderViewModel viewModel =
                    _orderService.GetPaymentOrderViewModel(order.OrderId, order.TableNumber);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                var errorViewModel = new ErrorViewModel
                {
                    ErrorMessage = ex.Message,
                    RequestId = HttpContext.TraceIdentifier
                };
                return View("Error", errorViewModel);
            }
        }

        public IActionResult CheckoutOrder(int orderId, int tableNumber)
        {
            try
            {
                PaymentOrderViewModel viewModel =
                    _orderService.GetPaymentOrderViewModel(orderId, tableNumber);
                return View("ViewOrder", viewModel);
            }
            catch (Exception ex)
            {
                var errorViewModel = new ErrorViewModel
                {
                    ErrorMessage = ex.Message,
                    RequestId = HttpContext.TraceIdentifier
                };
                return View("Error", errorViewModel);
            }
        }

        public IActionResult Confirmation(int orderId, decimal amount, string method)
        {
            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            ViewBag.Method = method;
            return View();
        }

        [HttpPost]
        public IActionResult SavePayment([FromBody] PaymentSaveRequest request)
        {
            try
            {
                if (request == null || request.OrderId <= 0)
                {
                    return Json(new { success = false, message = "Invalid order ID" });
                }

                string connectionString = _configuration.GetConnectionString("ChapeauDatabaseSQL");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. betaling dingen opslaan
                    string insertPaymentQuery = @"
                        INSERT INTO Payment (OrderID, TableID, TotalTipAmount, Feedback)
                        VALUES (@OrderID, @TableID, @TipAmount, @Feedback)";

                    using (SqlCommand command = new SqlCommand(insertPaymentQuery, connection))
                    {
                        command.Parameters.AddWithValue("@OrderID", request.OrderId);
                        command.Parameters.AddWithValue("@TableID", request.TableNumber);
                        command.Parameters.AddWithValue("@TipAmount", request.TipAmount);
                        command.Parameters.AddWithValue("@Feedback", (object?)request.Feedback ?? DBNull.Value);
                        command.ExecuteNonQuery();
                    }

                    // 2. markeer order naar betaald
                    string updateOrderQuery = "UPDATE Orders SET OrderStatus = 4 WHERE OrderID = @OrderID";
                    using (SqlCommand command = new SqlCommand(updateOrderQuery, connection))
                    {
                        command.Parameters.AddWithValue("@OrderID", request.OrderId);
                        command.ExecuteNonQuery();
                    }

                    // 3. tafel vrij maken
                    string updateTableQuery = @"
                        UPDATE Table_ 
                        SET TableStatus = 0  
                        WHERE TableID = (SELECT TableID FROM Orders WHERE OrderID = @OrderID)";

                    using (SqlCommand command = new SqlCommand(updateTableQuery, connection))
                    {
                        command.Parameters.AddWithValue("@OrderID", request.OrderId);
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Payment saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class PaymentSaveRequest
    {
        public int OrderId { get; set; }
        public int TableNumber { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TipAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Feedback { get; set; }
    }
}