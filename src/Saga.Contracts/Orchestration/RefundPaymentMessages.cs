namespace Saga.Contracts.Orchestration;

public record RefundPayment(Guid OrderId);

public record PaymentRefundedResult(Guid OrderId);
