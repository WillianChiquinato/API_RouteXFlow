public static class CorsExtensions
{
    public const string AllowRouteXFlowPolicy = "AllowRouteXFlow";

    public static IServiceCollection AddRouteXFlowCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = ResolveAllowedOrigins(configuration);

        services.AddCors(options =>
        {
            // [SEC] restrict CORS to known frontend origins
            options.AddPolicy(AllowRouteXFlowPolicy, builder =>
            {
                builder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    private static string[] ResolveAllowedOrigins(IConfiguration configuration)
    {
        var configuredOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        var legacyOrigin = configuration["Cors:AllowedOrigin"];
        if (!string.IsNullOrWhiteSpace(legacyOrigin))
        {
            configuredOrigins = configuredOrigins.Append(legacyOrigin).ToArray();
        }

        var normalizedOrigins = configuredOrigins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return normalizedOrigins.Length > 0
            ? normalizedOrigins
            : ["http://localhost:3000"];
    }
}
