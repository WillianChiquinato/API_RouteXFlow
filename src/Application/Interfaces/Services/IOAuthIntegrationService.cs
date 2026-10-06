using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Services;

public interface IOAuthIntegrationService
{
    Task<bool> SaveOAuthIntegrationAsync(OAuthIntegration integration);
    Task<OAuthIntegration?> GetIntegrationAsync(string provider, int userId);
    Task<bool> SaveExternalProfileAsync(ProfilesExternal profile);
    Task<string?> CreateStateAsync(string provider, int userId, TimeSpan lifetime, string? codeVerifier = null);
    Task<OAuthState?> ConsumeStateAsync(string state, string provider);
}
