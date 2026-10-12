namespace OrderService.Models;

public enum OrderStatus
{
    Created,
    Cancelled
}

public class Order
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }
}