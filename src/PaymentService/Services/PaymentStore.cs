using System.Collections.Concurrent;
using PaymentService.Models;

namespace PaymentService.Services;

public class PaymentStore
{
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();

    public Payment? Get(Guid orderId)
    {
        return _payments.GetValueOrDefault(orderId);
    }

    public Payment Charge(Guid orderId, decimal amount)
    {
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Completed
        };

        if (!_payments.TryAdd(orderId, payment))
            throw new InvalidOperationException("Payment already exists");

        return payment;
    }

    public bool Refund(Guid orderId)
    {
        if (!_payments.TryGetValue(orderId, out var payment))
            return false;

        payment.Status = PaymentStatus.Refunded;
        return true;
    }
}