namespace Saga.Contracts.Orchestration;

public record StartOrderSaga(
    Guid OrderId,
    decimal Amount,
    Guid ProductId,
    int Quantity,
    bool SimulatePaymentFailure,
    bool SimulateInventoryFailure
);