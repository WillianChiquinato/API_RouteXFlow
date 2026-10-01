using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Hosting;

public static class PresentationDependencyInjection
{
    public static bool ShouldEnableSwagger(IHostEnvironment environment)
    {
        return !environment.IsProduction();
    }

    // Basic Auth no /swagger só é exigido em Staging. Em Development o Swagger fica aberto.
    public static bool ShouldRequireSwaggerBasicAuth(IHostEnvironment environment)
    {
        return environment.IsStaging();
    }

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddEndpointsApiExplorer();
        services.AddRouteXFlowCors(configuration);

        services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(600)
            };
        });

        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app, IHostEnvironment environment)
    {
        // Em homolog/produção a aplicação fica atrás de um proxy/load balancer que termina o
        // HTTPS e repassa a requisição em HTTP puro para o Kestrel. Sem isso, Request.Scheme
        // vem sempre "http" (mesmo quando o cliente acessou via https://...), o que quebra
        // qualquer comparação que dependa da URL completa — ex.: a validação de assinatura
        // dos webhooks da Transfeera, que busca o signature_secret pela URL exata cadastrada.
        // Precisa vir antes de qualquer outro middleware que leia Scheme/Host/RemoteIp.
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };
        // Limpa a lista de proxies/redes conhecidas: em ambientes de nuvem o IP do proxy não é
        // fixo, então a validação padrão (que só aceita o header vindo de IPs conhecidos)
        // descartaria o header. A aplicação só é alcançável através desse proxy, então confiar
        // nele aqui é seguro.
        forwardedHeadersOptions.KnownNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();

        app.UseForwardedHeaders(forwardedHeadersOptions);

        if (ShouldEnableSwagger(app.Environment))
        {
            if (ShouldRequireSwaggerBasicAuth(app.Environment))
            {
                app.UseWhen(
                    context => context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase),
                    swaggerBranch =>
                    {
                        swaggerBranch.Use(async (context, next) =>
                        {
                            const string realm = "Swagger Homologacao";
                            var username = Environment.GetEnvironmentVariable("SWAGGER_USERNAME");
                            var password = Environment.GetEnvironmentVariable("SWAGGER_PASSWORD");

                            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                context.Response.Headers.Append("WWW-Authenticate", $"Basic realm=\"{realm}\"");
                                await context.Response.WriteAsync("Swagger protegido. Configure SWAGGER_USERNAME e SWAGGER_PASSWORD.");
                                return;
                            }

                            var authHeader = context.Request.Headers.Authorization.ToString();
                            if (!TryValidateBasicCredentials(authHeader, username, password))
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                context.Response.Headers.Append("WWW-Authenticate", $"Basic realm=\"{realm}\"");
                                await context.Response.WriteAsync("Credenciais inválidas.");
                                return;
                            }

                            await next();
                        });
                    });
            }

            if (!environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
        }

        return app;
    }

    private static bool TryValidateBasicCredentials(string authorizationHeader, string expectedUser, string expectedPassword)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var encodedCredentials = authorizationHeader["Basic ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(encodedCredentials))
        {
            return false;
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var separatorIndex = decoded.IndexOf(':');
            if (separatorIndex < 0)
            {
                return false;
            }

            var username = decoded[..separatorIndex];
            var password = decoded[(separatorIndex + 1)..];

            return string.Equals(username, expectedUser, StringComparison.Ordinal)
                && string.Equals(password, expectedPassword, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }
}