using PaymentService.Models;

namespace PaymentService.Services;

public class PaymentStore
{
    private readonly Dictionary<Guid, Payment> _payments = new();
    private readonly Lock _lock = new();

    public Payment? Get(Guid orderId)
    {
        lock (_lock)
        {
            return _payments.GetValueOrDefault(orderId);
        }
    }

    public Payment Charge(Guid orderId, decimal amount)
    {
        lock (_lock)
        {
            if (_payments.TryGetValue(orderId, out var existing))
            {
                if (existing.Amount != amount)
                    throw new InvalidOperationException("Payment amount mismatch");

                if (existing.Status == PaymentStatus.Refunded)
                    throw new InvalidOperationException("Payment already refunded");

                // Идемпотентность:
                // Не создаём новый платёж
                return existing;
            }

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = amount,
                Status = PaymentStatus.Completed
            };

            _payments.Add(orderId, payment);

            Console.WriteLine($"CHARGE executed: {orderId}");

            return payment;
        }
    }

    public bool Refund(Guid orderId)
    {
        lock (_lock)
        {
            if (!_payments.TryGetValue(orderId, out var payment))
                return false;

            if (payment.Status == PaymentStatus.Refunded)
            {
                // Повторный Refund ничего не делает
                return true;
            }

            payment.Status = PaymentStatus.Refunded;

            Console.WriteLine($"REFUND executed: {orderId}");

            return true;
        }
    }
}