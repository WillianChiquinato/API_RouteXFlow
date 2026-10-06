using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IOAuthIntegrationRepository
{
    Task<bool> SaveOAuthIntegrationAsync(OAuthIntegration integration);
    Task<OAuthIntegration?> GetIntegrationAsync(string provider, int userId);
    Task<bool> SaveExternalProfileAsync(ProfilesExternal profile);
    Task<bool> CreateStateAsync(OAuthState state);
    Task<OAuthState?> ConsumeStateAsync(string state, string provider);
}
