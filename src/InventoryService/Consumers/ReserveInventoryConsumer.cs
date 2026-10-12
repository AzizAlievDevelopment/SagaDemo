using InventoryService.Services;
using MassTransit;
using Saga.Contracts.Orchestration;

namespace InventoryService.Consumers;

public class ReserveInventoryConsumer(
    InventoryStore store,
    ILogger<ReserveInventoryConsumer> logger)
    : IConsumer<ReserveInventory>
{
    public async Task Consume(
        ConsumeContext<ReserveInventory> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "ReserveInventory received: {OrderId}",
            message.OrderId);

        if (message.SimulateFailure)
        {
            await context.Publish(
                new InventoryReservationFailedResult(
                    message.OrderId,
                    "Product is out of stock"));

            return;
        }

        try
        {
            store.Reserve(
                message.OrderId,
                message.ProductId,
                message.Quantity);

            await context.Publish(
                new InventoryReservedResult(message.OrderId));

            logger.LogInformation(
                "Inventory reserved: {OrderId}",
                message.OrderId);
        }
        catch (InvalidOperationException ex)
        {
            await context.Publish(
                new InventoryReservationFailedResult(
                    message.OrderId,
                    ex.Message));
        }
    }
}
