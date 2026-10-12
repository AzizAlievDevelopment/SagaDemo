var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Order Service is running");
app.MapGet("/health", () => Results.Ok(new
{
    Service = "OrderService",
    Status = "Healthy"
}));

app.Run();
