using System.Collections.Concurrent;

namespace Infrastructure.Audit;

public class AuditChange
{
    public string Entity { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object?> Key { get; set; } = new();
    public Dictionary<string, object?>? Before { get; set; }
    public Dictionary<string, object?>? After { get; set; }
}

/// <summary>
/// Coletor scoped por requisição: o interceptor do EF registra aqui o antes/depois
/// e o middleware lê no final para gravar no audit_log.
/// </summary>
public class AuditTrail
{
    private readonly ConcurrentQueue<AuditChange> _changes = new();

    public void Add(AuditChange change) => _changes.Enqueue(change);

    public IReadOnlyList<AuditChange> Snapshot() => _changes.ToArray();
}
