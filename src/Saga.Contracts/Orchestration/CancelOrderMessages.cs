
namespace Saga.Contracts.Orchestration;

public record CancelOrder(Guid OrderId);

public record OrderCancelledResult(Guid OrderId);
