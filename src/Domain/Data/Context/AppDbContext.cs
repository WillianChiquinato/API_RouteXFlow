using API_RouteXFlow.Domain.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {}

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RouteEvaluation> RouteEvaluations { get; set; }
    public DbSet<Container> Containers { get; set; }
    public DbSet<ContainerDevices> ContainerDevices { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<WorkSession> WorkSessions { get; set; }
    public DbSet<GpsPositionHistory> GpsPositions { get; set; }
    public DbSet<DeliveryOffers> DeliveryOffers { get; set; }
    public DbSet<DeliveryStops> DeliveryStops { get; set; }
    public DbSet<Deliveries> Deliveries { get; set; }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                // Set explicitly on add if not already set, though Postgres now() will also handle it
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;

                // Ensure CreatedAt is not modified
                entityEntry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
            }
        }
    }
}