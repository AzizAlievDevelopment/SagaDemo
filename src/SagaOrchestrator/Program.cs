using System.Text.Json;
using SagaOrchestrator.Models;
using SagaOrchestrator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<OrderSaga>(client => client.Timeout = TimeSpan.FromSeconds(10) );
builder.Services.AddSingleton<SagaIdempotencyStore>();

var app = builder.Build();

app.MapPost("/saga/orders",
    async (
        HttpContext context,
        CreateOrderRequest request,
        OrderSaga saga,
        SagaIdempotencyStore store) =>
    {
        if (request.Amount <= 0 ||
            request.Quantity <= 0 ||
            request.ProductId == Guid.Empty)
        {
            return Results.BadRequest("Invalid request");
        }

        var key = context.Request.Headers["Idempotency-Key"].ToString();

        if (string.IsNullOrWhiteSpace(key))
        {
            return Results.BadRequest("Idempotency-Key header is required");
        }

        // Одинаковый запрос создаёт одинаковый fingerprint.
        var fingerprint = JsonSerializer.Serialize(request);

        SagaResult result;

        try
        {
            result = await store.ExecuteAsync(key, fingerprint, () => saga.ExecuteAsync(request));
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new
            {
                Error = ex.Message
            });
        }

        return result.Status switch
        {
            "Completed" => Results.Ok(result),

            "Compensated" => Results.Json(result, statusCode: 409),

            _ => Results.Json(result, statusCode: 500)
        };
    });

app.Run();
