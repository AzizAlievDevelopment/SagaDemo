var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Payment Service is running");
app.MapGet("/health", () => Results.Ok(new
{
    Service = "PaymentService",
    Status = "Healthy"
}));


app.Run();
