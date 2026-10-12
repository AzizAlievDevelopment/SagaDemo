using MassTransit;
using OrderService.Services;
using Saga.Contracts.Orchestration;

namespace OrderService.Consumers;

public class CancelOrderConsumer(
    OrderStore orderStore,
    ILogger<CancelOrderConsumer> logger)
    : IConsumer<CancelOrder>
{
    public async Task Consume(
        ConsumeContext<CancelOrder> context)
    {
        var orderId = context.Message.OrderId;

        if (!orderStore.Cancel(orderId))
        {
            throw new InvalidOperationException(
                $"Order not found: {orderId}");
        }

        logger.LogInformation(
            "Order cancelled: {OrderId}",
            orderId);

        await context.Publish(
            new OrderCancelledResult(orderId));
    }
}
