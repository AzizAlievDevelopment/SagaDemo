
namespace Saga.Contracts.Orchestration;

public record CompleteOrder(Guid OrderId);

public record OrderCompletedResult(Guid OrderId);
