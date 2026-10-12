using SagaOrchestrator.Models;

namespace SagaOrchestrator.Services;

public class OrderSaga(HttpClient httpClient)
{
    private const string OrderUrl = "http://localhost:5001";
    private const string PaymentUrl = "http://localhost:5002";
    private const string InventoryUrl = "http://localhost:5003";

    public async Task<SagaResult> ExecuteAsync(
        CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid();
        var completedSteps = new Stack<string>();
        var history = new List<string>();
        var compensationErrors = new List<string>();

        try
        {
            // STEP 1: Create Order
            var orderResponse = await httpClient.PostAsJsonAsync($"{OrderUrl}/orders", new { OrderId = orderId, request.Amount });

            orderResponse.EnsureSuccessStatusCode();

            completedSteps.Push("Order");
            history.Add("Order created");

            // STEP 2: Charge Payment
            var paymentResponse = await httpClient.PostAsJsonAsync($"{PaymentUrl}/payments",
                new
                {
                    OrderId = orderId,
                    request.Amount,
                    SimulateFailure = request.SimulatePaymentFailure
                });

            paymentResponse.EnsureSuccessStatusCode();

            completedSteps.Push("Payment");
            history.Add("Payment completed");

            // STEP 3: Reserve Inventory
            var inventoryResponse = await httpClient.PostAsJsonAsync($"{InventoryUrl}/inventory/reserve",
                new
                {
                    OrderId = orderId,
                    request.ProductId,
                    request.Quantity,
                    SimulateFailure = request.SimulateInventoryFailure
                });

            inventoryResponse.EnsureSuccessStatusCode();

            completedSteps.Push("Inventory");
            history.Add("Inventory reserved");

            return new SagaResult(orderId, "Completed", history, compensationErrors);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            history.Add($"Saga failed: {ex.Message}");

            // Compensation in reverse order
            while (completedSteps.Count > 0)
            {
                var step = completedSteps.Pop();

                try
                {
                    HttpResponseMessage response = step switch
                    {
                        "Inventory" => await httpClient.PostAsync(
                            $"{InventoryUrl}/inventory/{orderId}/release",
                            null),

                        "Payment" => await httpClient.PostAsync(
                            $"{PaymentUrl}/payments/{orderId}/refund",
                            null),

                        "Order" => await httpClient.PostAsync(
                            $"{OrderUrl}/orders/{orderId}/cancel",
                            null),

                        _ => throw new InvalidOperationException()
                    };

                    response.EnsureSuccessStatusCode();
                    response.Dispose();

                    history.Add($"{step} compensated");
                }
                catch (Exception compensationEx)
                {
                    compensationErrors.Add($"{step}: {compensationEx.Message}");
                }
            }

            var status = compensationErrors.Count == 0
                ? "Compensated"
                : "CompensationFailed";

            return new SagaResult(orderId, status, history, compensationErrors);
        }
    }
}

public record SagaResult(
    Guid OrderId,
    string Status,
    List<string> History,
    List<string> CompensationErrors
);
