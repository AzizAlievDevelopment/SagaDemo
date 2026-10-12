using System.Collections.Concurrent;
using InventoryService.Models;

namespace InventoryService.Services;

public class InventoryStore
{
    private readonly ConcurrentDictionary<Guid, Reservation> _reservations = new();
    private readonly object _lock = new();

    public Reservation? Get(Guid orderId)
    {
        return _reservations.GetValueOrDefault(orderId);
    }


    public Reservation Reserve(
        Guid orderId,
        Guid productId,
        int quantity)
    {
        lock (_lock)
        {
            if (_reservations.TryGetValue(orderId, out var existing))
            {
                if (existing.ProductId != productId ||
                    existing.Quantity != quantity ||
                    existing.Status != ReservationStatus.Reserved)
                {
                    throw new InvalidOperationException(
                        "Reservation conflict");
                }

                // Повторная доставка: возвращаем существующий резерв
                return existing;
            }

            var reservation = new Reservation
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                Status = ReservationStatus.Reserved
            };

            if (!_reservations.TryAdd(orderId, reservation))
            {
                throw new InvalidOperationException(
                    "Reservation already exists");
            }

            return reservation;
        }
    }


    public bool Release(Guid orderId)
    {
        if (!_reservations.TryGetValue(orderId, out var reservation))
            return false;

        reservation.Status = ReservationStatus.Released;
        return true;
    }
}