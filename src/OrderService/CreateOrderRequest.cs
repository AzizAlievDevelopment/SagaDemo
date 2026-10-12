namespace OrderService;

public record CreateOrderRequest(Guid OrderId, decimal Amount);