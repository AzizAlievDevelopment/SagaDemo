namespace Saga.Contracts.Orchestration;

public record ReserveInventory(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    bool SimulateFailure
);

public record InventoryReservedResult(Guid OrderId);

public record InventoryReservationFailedResult(
    Guid OrderId,
    string Reason
);
