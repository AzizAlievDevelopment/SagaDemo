using SagaOrchestrator.Models;
using SagaOrchestrator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<OrderSaga>(client => { client.Timeout = TimeSpan.FromSeconds(10); });

var app = builder.Build();

app.MapPost("/saga/orders", async (CreateOrderRequest request, OrderSaga saga) =>
{
    if (request.Amount <= 0 || request.Quantity <= 0 || request.ProductId == Guid.Empty)
    {
        return Results.BadRequest("Invalid request");
    }

    var result = await saga.ExecuteAsync(request);

    return result.Status switch
    {
        "Completed" => Results.Ok(result),

        "Compensated" => Results.Json(result, statusCode: 409),

        _ => Results.Json(result, statusCode: 500)
    };
});

app.Run();