using MassTransit;
using PaymentService.Services;
using Saga.Contracts.Orchestration;

namespace PaymentService.Consumers;

public class ChargePaymentConsumer(
    PaymentStore paymentStore,
    ILogger<ChargePaymentConsumer> logger)
    : IConsumer<ChargePayment>
{
    public async Task Consume(
        ConsumeContext<ChargePayment> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "ChargePayment received: {OrderId}",
            message.OrderId);

        // Имитируем отказ оплаты до списания
        if (message.SimulateFailure)
        {
            await context.Publish(new PaymentChargeFailed(
                message.OrderId,
                "Payment declined"));

            return;
        }

        try
        {
            paymentStore.Charge(
                message.OrderId,
                message.Amount);

            await context.Publish(
                new PaymentCharged(message.OrderId));

            logger.LogInformation(
                "Payment charged: {OrderId}",
                message.OrderId);
        }
        catch (InvalidOperationException ex)
        {
            await context.Publish(new PaymentChargeFailed(
                message.OrderId,
                ex.Message));
        }
    }
}
