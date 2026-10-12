namespace PaymentService;

record CreatePaymentRequest(
    Guid OrderId,
    decimal Amount,
    bool SimulateFailure = false);
