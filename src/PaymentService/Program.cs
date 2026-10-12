using MassTransit;
using PaymentService;
using PaymentService.Consumers;
using PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PaymentStore>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ChargePaymentConsumer>();
    x.AddConsumer<RefundPaymentConsumer>();
    
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


app.MapPost("/payments", (CreatePaymentRequest request, PaymentStore store) =>
{
    if (request.Amount <= 0 || request.OrderId == Guid.Empty)
        return Results.BadRequest("Invalid payment request");

    if (request.SimulateFailure)
    {
        return Results.Conflict(new
        {
            Error = "Payment declined"
        });
    }

    try
    {
        var existing = store.Get(request.OrderId);

        var payment = store.Charge(request.OrderId, request.Amount);

        return existing is null
            ? Results.Created($"/payments/{payment.OrderId}", payment)
            : Results.Ok(payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new
        {
            Error = ex.Message
        });
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