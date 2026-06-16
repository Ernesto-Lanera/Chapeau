using Chapeau.Services.Payment;
using Chapeau.Repositories.Payment;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public void SavePayment(int orderId, decimal tipAmount, string? feedback)
    {
        if (orderId <= 0)
            throw new ArgumentException("Ongeldig order ID.", nameof(orderId));

        _paymentRepository.SavePayment(orderId, tipAmount, feedback);
    }
}