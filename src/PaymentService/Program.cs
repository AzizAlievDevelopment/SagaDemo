using PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PaymentStore>();

var app = builder.Build();

app.MapPost("/payments", (CreatePaymentRequest request, PaymentStore store) =>
{
    if (request.Amount <= 0)
        return Results.BadRequest("Invalid amount");

    try
    {
        var payment = store.Charge(
            request.OrderId,
            request.Amount);

        return Results.Created($"/payments/{payment.OrderId}", payment);
    }
    catch (InvalidOperationException)
    {
        return Results.Conflict("Payment already exists");
    }
});

app.MapGet("/payments/{orderId:guid}", (Guid orderId, PaymentStore store) =>
{
    var payment = store.Get(orderId);

    return payment is null
        ? Results.NotFound()
        : Results.Ok(payment);
});

app.MapPost("/payments/{orderId:guid}/refund", (Guid orderId, PaymentStore store) =>
{
    var refunded = store.Refund(orderId);

    return refunded
        ? Results.Ok(new { Status = "Refunded" })
        : Results.NotFound();
});

app.Run();