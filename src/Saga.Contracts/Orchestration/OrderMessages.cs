
namespace Saga.Contracts.Orchestration;

// Команда от Orchestrator к OrderService
public record CreateOrder(
    Guid OrderId,
    decimal Amount
);

// Результат успешного создания
public record OrderCreatedResult(Guid OrderId);

// Результат ошибки создания
public record OrderCreationFailed(
    Guid OrderId,
    string Reason
);
