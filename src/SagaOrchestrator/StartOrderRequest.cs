record StartOrderRequest(
    decimal Amount,
    Guid ProductId,
    int Quantity,
    bool SimulatePaymentFailure = false,
    bool SimulateInventoryFailure = false
);