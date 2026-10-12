namespace Saga.Contracts.Events;

public record PaymentCompleted(
    Guid OrderId,
    decimal Amount,
    Guid ProductId,
    int Quantity,
    bool SimulateInventoryFailure
);

public record PaymentFailed(Guid OrderId, string Reason);

public record PaymentRefunded(Guid OrderId);