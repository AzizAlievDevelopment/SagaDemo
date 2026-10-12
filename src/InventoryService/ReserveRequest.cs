record ReserveRequest(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    bool SimulateFailure = false);