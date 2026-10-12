namespace OrderService;
record CreateOrderRequest(
    Guid OrderId,
    decimal Amount,
    Guid ProductId,
    int Quantity,
    bool SimulatePaymentFailure = false,
    bool SimulateInventoryFailure = false
);
