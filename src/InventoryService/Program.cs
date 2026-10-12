using InventoryService.Consumers;
using InventoryService.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<InventoryStore>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentCompletedConsumer>();
    
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

app.MapPost("/inventory/reserve", (ReserveRequest request, InventoryStore store) =>
{
    if (request.Quantity <= 0)
        return Results.BadRequest("Invalid quantity");

    // Искусственная ошибка для тестирования Saga
    if (request.SimulateFailure)
    {
        return Results.Conflict(new { Error = "Product is out of stock" });
    }

    try
    {
        var reservation = store.Reserve(
            request.OrderId,
            request.ProductId,
            request.Quantity);

        return Results.Created($"/inventory/{reservation.OrderId}", reservation);
    }
    catch (InvalidOperationException)
    {
        return Results.Conflict("Reservation already exists");
    }
});

app.MapGet("/inventory/{orderId:guid}", (Guid orderId, InventoryStore store) =>
{
    var reservation = store.Get(orderId);

    return reservation is null
        ? Results.NotFound()
        : Results.Ok(reservation);
});

app.MapPost("/inventory/{orderId:guid}/release", (Guid orderId, InventoryStore store) =>
{
    var released = store.Release(orderId);

    return released
        ? Results.Ok(new { Status = "Released" })
        : Results.NotFound();
});

app.Run();