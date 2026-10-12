
using MassTransit;

namespace SagaOrchestrator.StateMachines;

public class OrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }

    public string CurrentState { get; set; } = null!;

    public decimal Amount { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public bool SimulatePaymentFailure { get; set; }

    public bool SimulateInventoryFailure { get; set; }
}
