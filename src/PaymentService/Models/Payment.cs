namespace PaymentService.Models;

public enum PaymentStatus
{
    Completed,
    Refunded
}

public class Payment
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
}