namespace Saga.Contracts.Events;

public record InventoryReserved(Guid OrderId);

public record InventoryReservationFailed(
    Guid OrderId,
    string Reason
);