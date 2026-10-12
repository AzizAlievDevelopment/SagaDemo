using MassTransit;
using OrderService.Services;
using Saga.Contracts.Events;

namespace OrderService.Consumers;

public class InventoryReservedConsumer(
    OrderStore orderStore,
    IPublishEndpoint publisher,
    ILogger<InventoryReservedConsumer> logger)
    : IConsumer<InventoryReserved>
{
    public async Task Consume(
        ConsumeContext<InventoryReserved> context)
    {
        var orderId = context.Message.OrderId;

        var completed = orderStore.Complete(orderId);

        if (!completed)
        {
            throw new InvalidOperationException(
                $"Cannot complete order: {orderId}");
        }

        logger.LogInformation(
            "Order completed: {OrderId}",
            orderId);

        await publisher.Publish(new OrderCompleted(orderId));
    }
}