using System.Collections.Concurrent;

namespace SagaOrchestrator.Services;

public sealed class SagaIdempotencyStore
{
    private readonly ConcurrentDictionary<string, Entry> _entries = new();

    public Task<SagaResult> ExecuteAsync(string key, string fingerprint, Func<Task<SagaResult>> execute)
    {
        var candidate = new Entry(
            fingerprint,
            new Lazy<Task<SagaResult>>(execute, LazyThreadSafetyMode.ExecutionAndPublication));

        var entry = _entries.GetOrAdd(key, candidate);

        if (entry.Fingerprint != fingerprint)
        {
            throw new InvalidOperationException("Idempotency-Key already used with another request");
        }

        return entry.Execution.Value;
    }

    private sealed record Entry(string Fingerprint, Lazy<Task<SagaResult>> Execution);
}
