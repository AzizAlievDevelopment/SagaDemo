using MassTransit;
using PaymentService.Services;
using Saga.Contracts.Events;

namespace PaymentService.Consumers;

public class OrderCreatedConsumer(PaymentStore paymentStore, ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var message = context.Message;

        logger.LogInformation("Received OrderCreated: {OrderId}", message.OrderId);

        if (message.SimulatePaymentFailure)
        {
            logger.LogWarning("Payment failed: {OrderId}", message.OrderId);
            await context.Publish(new PaymentFailed(message.OrderId, "Payment declined"));
            return;
        }

        var payment = paymentStore.Charge(message.OrderId, message.Amount);
        logger.LogInformation("Payment completed: {OrderId}", payment.OrderId);

        await context.Publish(new PaymentCompleted(
            message.OrderId,
            message.Amount,
            message.ProductId,
            message.Quantity,
            message.SimulateInventoryFailure
        ));
    }
}