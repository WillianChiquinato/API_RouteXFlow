using Microsoft.Extensions.DependencyInjection;

namespace API_RouteXFlow.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProjectDependencies(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromApplicationDependencies()
                .AddClasses(c => c.Where(t =>
                    t.Namespace?.StartsWith("API_RouteXFlow", StringComparison.Ordinal) == true &&
                    t.Name.EndsWith("Service", StringComparison.Ordinal)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            );

            services.Scan(scan => scan
                .FromApplicationDependencies()
                .AddClasses(c => c.Where(t =>
                    t.Namespace?.StartsWith("API_RouteXFlow", StringComparison.Ordinal) == true &&
                    t.Name.EndsWith("Repository", StringComparison.Ordinal)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            );

            return services;
        }
    }
}