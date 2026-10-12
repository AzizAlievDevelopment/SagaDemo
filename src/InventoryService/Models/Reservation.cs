namespace InventoryService.Models;

public enum ReservationStatus
{
    Reserved,
    Released
}

public class Reservation
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public ReservationStatus Status { get; set; }
}