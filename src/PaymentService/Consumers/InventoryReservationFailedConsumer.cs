using MassTransit;
using PaymentService.Services;
using Saga.Contracts.Events;

namespace PaymentService.Consumers;

public class InventoryReservationFailedConsumer(
    PaymentStore paymentStore,
    ILogger<InventoryReservationFailedConsumer> logger)
    : IConsumer<InventoryReservationFailed>
{
    public async Task Consume(
        ConsumeContext<InventoryReservationFailed> context)
    {
        var message = context.Message;

        logger.LogWarning(
            "Inventory failed for order {OrderId}: {Reason}",
            message.OrderId,
            message.Reason);

        var refunded = paymentStore.Refund(message.OrderId);

        if (!refunded)
        {
            // Не публикуем подтверждение возврата,
            // если платёж не найден.
            throw new InvalidOperationException(
                $"Payment not found: {message.OrderId}");
        }

        logger.LogInformation(
            "Payment refunded: {OrderId}",
            message.OrderId);

        await context.Publish(
            new PaymentRefunded(message.OrderId));
    }
}