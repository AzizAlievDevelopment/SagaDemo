namespace Saga.Contracts.Events;

public record OrderCreated(
    Guid OrderId,
    decimal Amount,
    Guid ProductId,
    int Quantity,
    bool SimulatePaymentFailure,
    bool SimulateInventoryFailure
);

public record OrderCompleted(Guid OrderId);

public record OrderCancelled(Guid OrderId);