using MassTransit;
using OrderService;
using OrderService.Consumers;
using OrderService.Services;
using Saga.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<OrderStore>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CreateOrderConsumer>();
    x.AddConsumer<CancelOrderConsumer>();
    x.AddConsumer<CompleteOrderConsumer>();
    
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

// app.MapPost("/orders",
//     async (
//         CreateOrderRequest request,
//         OrderStore store,
//         IPublishEndpoint publisher) =>
//     {
//         if (request.Amount <= 0)
//             return Results.BadRequest("Invalid amount");
//
//         var orderId = request.OrderId == Guid.Empty
//             ? Guid.NewGuid()
//             : request.OrderId;
//
//         try
//         {
//             var order = store.Create(
//                 orderId,
//                 request.Amount);
//
//             await publisher.Publish(new OrderCreated(
//                 orderId,
//                 request.Amount,
//                 request.ProductId,
//                 request.Quantity,
//                 request.SimulatePaymentFailure,
//                 request.SimulateInventoryFailure
//             ));
//
//             return Results.Accepted(
//                 $"/orders/{orderId}",
//                 new
//                 {
//                     OrderId = orderId,
//                     Status = "Processing"
//                 });
//         }
//         catch (InvalidOperationException ex)
//         {
//             return Results.Conflict(ex.Message);
//         }
//     });


app.MapGet("/orders/{id:guid}", (Guid id, OrderStore store) =>
{
    var order = store.Get(id);
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.MapPost("/orders/{id:guid}/cancel", (Guid id, OrderStore store) =>
{
    var cancelled = store.Cancel(id);

    return cancelled
        ? Results.Ok(new { Status = "Cancelled" })
        : Results.NotFound();
});

app.Run();