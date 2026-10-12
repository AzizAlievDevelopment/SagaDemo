using MassTransit;
using OrderService.Services;
using Saga.Contracts.Events;

namespace OrderService.Consumers;

public class PaymentFailedConsumer(
    OrderStore orderStore,
    ILogger<PaymentFailedConsumer> logger)
    : IConsumer<PaymentFailed>
{
    public Task Consume(ConsumeContext<PaymentFailed> context)
    {
        var orderId = context.Message.OrderId;

        if (!orderStore.Cancel(orderId))
        {
            throw new InvalidOperationException(
                $"Order not found: {orderId}");
        }

        logger.LogWarning(
            "Order cancelled after payment failure: {OrderId}",
            orderId);

        return Task.CompletedTask;
    }
}