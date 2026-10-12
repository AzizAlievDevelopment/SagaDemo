using MassTransit;
using OrderService.Services;
using Saga.Contracts.Orchestration;

namespace OrderService.Consumers;

public class CompleteOrderConsumer(
    OrderStore store,
    ILogger<CompleteOrderConsumer> logger)
    : IConsumer<CompleteOrder>
{
    public async Task Consume(
        ConsumeContext<CompleteOrder> context)
    {
        var orderId = context.Message.OrderId;

        if (!store.Complete(orderId))
        {
            throw new InvalidOperationException(
                $"Cannot complete order: {orderId}");
        }

        logger.LogInformation(
            "Order completed: {OrderId}",
            orderId);

        await context.Publish(
            new OrderCompletedResult(orderId));
    }
}
