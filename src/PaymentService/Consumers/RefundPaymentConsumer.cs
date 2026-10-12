using MassTransit;
using PaymentService.Services;
using Saga.Contracts.Orchestration;

namespace PaymentService.Consumers;

public class RefundPaymentConsumer(
    PaymentStore store,
    ILogger<RefundPaymentConsumer> logger)
    : IConsumer<RefundPayment>
{
    public async Task Consume(
        ConsumeContext<RefundPayment> context)
    {
        var orderId = context.Message.OrderId;

        var refunded = store.Refund(orderId);

        if (!refunded)
        {
            // Не подтверждаем возврат,
            // если платёж отсутствует.
            throw new InvalidOperationException(
                $"Payment not found: {orderId}");
        }

        logger.LogInformation(
            "Payment refunded: {OrderId}",
            orderId);

        await context.Publish(
            new PaymentRefundedResult(orderId));
    }
}
