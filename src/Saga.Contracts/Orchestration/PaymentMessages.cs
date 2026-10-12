namespace Saga.Contracts.Orchestration;

// Команда: провести оплату
public record ChargePayment(
    Guid OrderId,
    decimal Amount,
    bool SimulateFailure
);

// Результат: оплата прошла
public record PaymentCharged(Guid OrderId);

// Результат: оплата отклонена
public record PaymentChargeFailed(
    Guid OrderId,
    string Reason
);
