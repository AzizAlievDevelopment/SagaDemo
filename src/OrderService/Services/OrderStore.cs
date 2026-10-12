using System.Collections.Concurrent;
using OrderService.Models;

namespace OrderService.Services;

public class OrderStore
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Order Create(Guid id, decimal amount)
    {
        var order = new Order
        {
            Id = id,
            Amount = amount,
            Status = OrderStatus.Created
        };

        if (!_orders.TryAdd(id, order))
            throw new InvalidOperationException("Order already exists");

        return order;
    }

    public Order? Get(Guid id)
    {
        return _orders.GetValueOrDefault(id);
    }

    public bool Cancel(Guid id)
    {
        if (!_orders.TryGetValue(id, out var order))
            return false;

        order.Status = OrderStatus.Cancelled;
        return true;
    }
    
    

    public bool Complete(Guid orderId)
    {
        if (!_orders.TryGetValue(orderId, out var order))
            return false;

        if (order.Status == OrderStatus.Cancelled)
            return false;

        order.Status = OrderStatus.Completed;
        return true;
    }
}