var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Saga Orchestrator is running");
app.MapGet("/health", () => Results.Ok(new
{
    Service = "SagaOrchestrator",
    Status = "Healthy"
}));

app.Run();
