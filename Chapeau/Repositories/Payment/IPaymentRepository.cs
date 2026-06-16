namespace Chapeau.Repositories.Payment
{
    public interface IPaymentRepository
    {
        void SavePayment(int orderId, decimal tipAmount, string? feedback);
    }
}
