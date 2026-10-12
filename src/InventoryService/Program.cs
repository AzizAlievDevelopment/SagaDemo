var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Inventory Service is running");
app.MapGet("/health", () => Results.Ok(new
{
    Service = "InventoryService",
    Status = "Healthy"
}));


app.Run();
