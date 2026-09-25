using API_RouteXFlow.Domain.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {}

    public DbSet<Apps> Apps { get; set; }
    public DbSet<AppsVinculatedUser> AppsVinculatedUser { get; set; }
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
    public DbSet<FinanceEntry> FinanceEntries { get; set; }
    public DbSet<FinanceMonthClosure> FinanceMonthClosures { get; set; }

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

    //SEEDS  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Status>().HasData(
            new Status { Id = 11, Name = "Ativo", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Status { Id = 12, Name = "Inativo", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Status { Id = 13, Name = "Suspenso", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Status { Id = 14, Name = "Aprovado", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Status { Id = 15, Name = "Reprovado", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Status { Id = 16, Name = "Em Análise", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Status { Id = 17, Name = "Pendente", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Status { Id = 18, Name = "Em Andamento", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Status { Id = 19, Name = "Concluído", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Status { Id = 20, Name = "Cancelado", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Super-Admin", Description = "Perfil feito para Desenvolvedores e administradores gerais",CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 2, Name = "Admin", Description = "Perfil feito para administradores comuns",CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 3, Name = "Operação", Description = "Perfil para operadores", CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<FinanceEntry>().Property(e => e.Type).HasConversion<string>();
        modelBuilder.Entity<FinanceEntry>().Property(e => e.Source).HasConversion<string>();
        modelBuilder.Entity<FinanceEntry>().Property(e => e.Category).HasConversion<string>();
        modelBuilder.Entity<FinanceEntry>().HasIndex(e => new { e.UserId, e.Date });

        modelBuilder.Entity<FinanceMonthClosure>().HasIndex(c => new { c.UserId, c.Year, c.Month }).IsUnique();

        modelBuilder.Entity<Apps>().HasData(
            new Apps { Id = 1, Name = "Ifood", Description = "Aplicativo Vermelho de entregas", IconUrl = "", TypeApps = TypeApps.Delivery, CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Apps { Id = 2, Name = "99Food", Description = "Aplicativo Amarelo de entregas", IconUrl = "", TypeApps = TypeApps.Delivery, CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Apps { Id = 3, Name = "Keeta", Description = "Aplicativo Verde e Amarelo de entregas", IconUrl = "", TypeApps = TypeApps.Delivery, CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Apps { Id = 4, Name = "Shoppe", Description = "Aplicativo vermelho de entregas", IconUrl = "", TypeApps = TypeApps.MarketPlace, CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Apps { Id = 5, Name = "Mercado Livre", Description = "Aplicativo Amarelo de entregas", IconUrl = "", TypeApps = TypeApps.MarketPlace, CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) }
        );
    }
}