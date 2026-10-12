using OrderService;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<OrderStore>();

var app = builder.Build();

app.MapPost("/orders", (CreateOrderRequest request, OrderStore store) => 
{
    if (request.Amount <= 0)
        return Results.BadRequest("Invalid amount");

    try
    {
        var order = store.Create(request.OrderId, request.Amount);
        return Results.Created($"/orders/{order.Id}", order);
    }
    catch (InvalidOperationException)
    {
        return Results.Conflict("Order already exists");
    }
});

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