using InventoryService.Services;
using MassTransit;
using Saga.Contracts.Events;

namespace InventoryService.Consumers;

public class PaymentCompletedConsumer(
    InventoryStore inventoryStore,
    ILogger<PaymentCompletedConsumer> logger)
    : IConsumer<PaymentCompleted>
{
    public async Task Consume(
        ConsumeContext<PaymentCompleted> context)
    {
        var message = context.Message;

        logger.LogInformation("Received PaymentCompleted: {OrderId}", message.OrderId);

        // Искусственная ошибка для тестирования Saga
        if (message.SimulateInventoryFailure)
        {
            logger.LogWarning(
                "Inventory failed: {OrderId}",
                message.OrderId);

            await context.Publish(
                new InventoryReservationFailed(
                    message.OrderId,
                    "Product is out of stock"
                ));

            return;
        }

        try
        {
            // Резервируем товар
            var reservation = inventoryStore.Reserve(
                message.OrderId,
                message.ProductId,
                message.Quantity);

            logger.LogInformation(
                "Inventory reserved: {OrderId}",
                reservation.OrderId);

            // Публикуем событие об успешном резервировании
            await context.Publish(
                new InventoryReserved(message.OrderId));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(
                ex,
                "Inventory reservation rejected: {OrderId}",
                message.OrderId);

            await context.Publish(
                new InventoryReservationFailed(
                    message.OrderId,
                    ex.Message
                ));
        }
    }
}