
using MassTransit;
using Saga.Contracts.Orchestration;
using SagaOrchestrator.StateMachines;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<
        OrderSagaStateMachine,
        OrderSagaState>()
        .InMemoryRepository();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("saga");
            h.Password("saga123");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapPost("/saga/orders",
    async (
        StartOrderRequest request,
        IPublishEndpoint publisher) =>
    {
        if (request.Amount <= 0 ||
            request.Quantity <= 0 ||
            request.ProductId == Guid.Empty)
        {
            return Results.BadRequest("Invalid request");
        }

        var orderId = Guid.NewGuid();

        await publisher.Publish(new StartOrderSaga(
            orderId,
            request.Amount,
            request.ProductId,
            request.Quantity,
            request.SimulatePaymentFailure,
            request.SimulateInventoryFailure
        ));

        return Results.Accepted(
            value: new
            {
                OrderId = orderId,
                Status = "Accepted"
            });
    });

app.Run();