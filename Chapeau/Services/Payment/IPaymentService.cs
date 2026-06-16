namespace Chapeau.Services.Payment
{
    public interface IPaymentService
    {
        void SavePayment(int orderId, decimal tipAmount, string? feedback);
    }
}
