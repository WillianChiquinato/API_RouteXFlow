using API_RouteXFlow.Domain.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Audit;

/// <summary>
/// Captura o estado antes/depois de cada entidade gravada via SaveChanges.
/// Obs.: ExecuteUpdate/ExecuteDelete não passam pelo ChangeTracker e não são capturados.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> IgnoredProperties = new() { "CreatedAt", "UpdatedAt" };

    private readonly AuditTrail _trail;
    private readonly List<(EntityEntry Entry, AuditChange Change)> _pending = new();

    public AuditSaveChangesInterceptor(AuditTrail trail) => _trail = trail;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Commit();
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Commit();
        return base.SavedChanges(eventData, result);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _pending.Clear();
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _pending.Clear();
        base.SaveChangesFailed(eventData);
    }

    private void Capture(DbContext? context)
    {
        _pending.Clear();
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog) continue;
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

            var change = new AuditChange
            {
                Entity = entry.Metadata.ClrType.Name,
                Action = entry.State.ToString().ToUpperInvariant()
            };

            foreach (var prop in entry.Properties)
            {
                var name = prop.Metadata.Name;
                if (IgnoredProperties.Contains(name)) continue;
                var sensitive = AuditRedactor.IsSensitive(name);

                switch (entry.State)
                {
                    case EntityState.Added:
                        (change.After ??= new())[name] = sensitive ? "***" : prop.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        (change.Before ??= new())[name] = sensitive ? "***" : prop.OriginalValue;
                        break;
                    case EntityState.Modified when prop.IsModified:
                        (change.Before ??= new())[name] = sensitive ? "***" : prop.OriginalValue;
                        (change.After ??= new())[name] = sensitive ? "***" : prop.CurrentValue;
                        break;
                }
            }

            // Modified sem nenhuma coluna relevante alterada (ex.: só UpdatedAt) não interessa.
            if (entry.State == EntityState.Modified && change.After is null) continue;

            _pending.Add((entry, change));
        }
    }

    private void Commit()
    {
        foreach (var (entry, change) in _pending)
        {
            // PK só existe depois do INSERT (identity).
            var key = entry.Metadata.FindPrimaryKey();
            if (key is not null)
            {
                foreach (var p in key.Properties)
                    change.Key[p.Name] = entry.Property(p.Name).CurrentValue;
            }

            _trail.Add(change);
        }

        _pending.Clear();
    }
}
