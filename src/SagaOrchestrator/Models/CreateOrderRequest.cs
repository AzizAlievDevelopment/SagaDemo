namespace SagaOrchestrator.Models;

public record CreateOrderRequest(
    decimal Amount,
    Guid ProductId,
    int Quantity,
    bool SimulateInventoryFailure = false
);