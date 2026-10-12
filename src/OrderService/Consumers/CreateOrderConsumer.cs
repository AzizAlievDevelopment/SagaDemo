
using MassTransit;
using OrderService.Services;
using Saga.Contracts.Orchestration;

namespace OrderService.Consumers;

public class CreateOrderConsumer(
    OrderStore orderStore,
    ILogger<CreateOrderConsumer> logger)
    : IConsumer<CreateOrder>
{
    public async Task Consume(
        ConsumeContext<CreateOrder> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "CreateOrder received: {OrderId}",
            message.OrderId);

        try
        {
            var existing = orderStore.Get(message.OrderId);

            if (existing is null)
            {
                orderStore.Create(
                    message.OrderId,
                    message.Amount);
            }
            else if (existing.Amount != message.Amount)
            {
                throw new InvalidOperationException(
                    "Order amount mismatch");
            }

            await context.Publish(
                new OrderCreatedResult(message.OrderId));

            logger.LogInformation(
                "Order created successfully: {OrderId}",
                message.OrderId);
        }
        catch (InvalidOperationException ex)
        {
            await context.Publish(
                new OrderCreationFailed(
                    message.OrderId,
                    ex.Message));
        }
    }
}
