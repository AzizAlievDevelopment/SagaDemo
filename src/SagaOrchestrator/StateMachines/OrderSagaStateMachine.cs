using MassTransit;
using Saga.Contracts.Orchestration;

namespace SagaOrchestrator.StateMachines;

public class OrderSagaStateMachine
    : MassTransitStateMachine<OrderSagaState>
{
    // Состояния
    public State WaitingForOrder { get; private set; } = null!;
    public State WaitingForPayment { get; private set; } = null!;
    public State WaitingForInventory { get; private set; } = null!;
    public State WaitingForCompletion { get; private set; } = null!;
    public State WaitingForRefund { get; private set; } = null!;
    public State WaitingForCancellation { get; private set; } = null!;

    public State Completed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;
    public State OrderFailed { get; private set; } = null!;

    // События
    public Event<StartOrderSaga> StartOrder { get; private set; } = null!;
    public Event<OrderCreatedResult> OrderCreatedEvent { get; private set; } = null!;
    public Event<OrderCreationFailed> OrderFailedEvent { get; private set; } = null!;
    public Event<PaymentCharged> PaymentChargedEvent { get; private set; } = null!;
    public Event<PaymentChargeFailed> PaymentFailedEvent { get; private set; } = null!;
    public Event<InventoryReservedResult> InventoryReservedEvent { get; private set; } = null!;
    public Event<InventoryReservationFailedResult> InventoryFailedEvent { get; private set; } = null!;
    public Event<PaymentRefundedResult> PaymentRefundedEvent { get; private set; } = null!;
    public Event<OrderCancelledResult> OrderCancelledEvent { get; private set; } = null!;
    public Event<OrderCompletedResult> OrderCompletedEvent { get; private set; } = null!;

    public OrderSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Все сообщения сопоставляются по OrderId
        Event(() => StartOrder,
            x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => OrderCreatedEvent,
            x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => OrderFailedEvent,
            x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => PaymentChargedEvent,
            x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => PaymentFailedEvent,
            x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => InventoryReservedEvent,
            x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => InventoryFailedEvent,
            x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => PaymentRefundedEvent,
            x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => OrderCancelledEvent,
            x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => OrderCompletedEvent,
            x => x.CorrelateById(m => m.Message.OrderId));

        // 1. START SAGA
        Initially(
            When(StartOrder)
                .Then(ctx =>
                {
                    ctx.Saga.Amount = ctx.Message.Amount;
                    ctx.Saga.ProductId = ctx.Message.ProductId;
                    ctx.Saga.Quantity = ctx.Message.Quantity;
                    ctx.Saga.SimulatePaymentFailure =
                        ctx.Message.SimulatePaymentFailure;
                    ctx.Saga.SimulateInventoryFailure =
                        ctx.Message.SimulateInventoryFailure;

                    Console.WriteLine(
                        $"Saga started: {ctx.Saga.CorrelationId}");
                })
                .Publish(ctx => new CreateOrder(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.Amount))
                .TransitionTo(WaitingForOrder)
        );

        // 2. CREATE ORDER
        During(WaitingForOrder,
            When(OrderCreatedEvent)
                .Then(ctx => Console.WriteLine("Order created"))
                .Publish(ctx => new ChargePayment(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.Amount,
                    ctx.Saga.SimulatePaymentFailure))
                .TransitionTo(WaitingForPayment),

            When(OrderFailedEvent)
                .Then(ctx => Console.WriteLine(
                    $"Order failed: {ctx.Message.Reason}"))
                .TransitionTo(OrderFailed)
        );

        // 3. CHARGE PAYMENT
        During(WaitingForPayment,
            When(PaymentChargedEvent)
                .Then(ctx => Console.WriteLine("Payment charged"))
                .Publish(ctx => new ReserveInventory(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.ProductId,
                    ctx.Saga.Quantity,
                    ctx.Saga.SimulateInventoryFailure))
                .TransitionTo(WaitingForInventory),

            When(PaymentFailedEvent)
                .Then(ctx => Console.WriteLine(
                    $"Payment failed: {ctx.Message.Reason}"))
                .Publish(ctx => new CancelOrder(
                    ctx.Saga.CorrelationId))
                .TransitionTo(WaitingForCancellation)
        );

        // 4. RESERVE INVENTORY
        During(WaitingForInventory,
            When(InventoryReservedEvent)
                .Then(ctx => Console.WriteLine("Inventory reserved"))
                .Publish(ctx => new CompleteOrder(
                    ctx.Saga.CorrelationId))
                .TransitionTo(WaitingForCompletion),

            When(InventoryFailedEvent)
                .Then(ctx => Console.WriteLine(
                    $"Inventory failed: {ctx.Message.Reason}"))
                .Publish(ctx => new RefundPayment(
                    ctx.Saga.CorrelationId))
                .TransitionTo(WaitingForRefund)
        );

        // 5. SUCCESSFUL COMPLETION
        During(WaitingForCompletion,
            When(OrderCompletedEvent)
                .Then(ctx => Console.WriteLine(
                    $"Saga COMPLETED: {ctx.Saga.CorrelationId}"))
                .TransitionTo(Completed)
        );

        // 6. PAYMENT REFUND
        During(WaitingForRefund,
            When(PaymentRefundedEvent)
                .Then(ctx => Console.WriteLine("Payment refunded"))
                .Publish(ctx => new CancelOrder(
                    ctx.Saga.CorrelationId))
                .TransitionTo(WaitingForCancellation)
        );

        // 7. CANCEL ORDER
        During(WaitingForCancellation,
            When(OrderCancelledEvent)
                .Then(ctx => Console.WriteLine(
                    $"Saga CANCELLED: {ctx.Saga.CorrelationId}"))
                .TransitionTo(Cancelled)
        );
    }
}
