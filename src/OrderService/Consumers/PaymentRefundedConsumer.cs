using MassTransit;
using OrderService.Services;
using Saga.Contracts.Events;

namespace OrderService.Consumers;

public class PaymentRefundedConsumer(
    OrderStore orderStore,
    ILogger<PaymentRefundedConsumer> logger)
    : IConsumer<PaymentRefunded>
{
    public Task Consume(
        ConsumeContext<PaymentRefunded> context)
    {
        var orderId = context.Message.OrderId;

        var cancelled = orderStore.Cancel(orderId);

        if (!cancelled)
        {
            throw new InvalidOperationException(
                $"Order not found: {orderId}");
        }

        logger.LogInformation(
            "Order cancelled: {OrderId}",
            orderId);

        return Task.CompletedTask;
    }
}