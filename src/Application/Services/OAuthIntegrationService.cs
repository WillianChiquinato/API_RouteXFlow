using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class OAuthIntegrationService : IOAuthIntegrationService
{
    private readonly ILogger<OAuthIntegrationService> _logger;
    private readonly IOAuthIntegrationRepository _oAuthIntegrationRepository;

    public OAuthIntegrationService(ILogger<OAuthIntegrationService> logger, IOAuthIntegrationRepository oAuthIntegrationRepository)
    {
        _logger = logger;
        _oAuthIntegrationRepository = oAuthIntegrationRepository;
    }

    public async Task<bool> SaveOAuthIntegrationAsync(OAuthIntegration integration)
    {
        try
        {
            var result = await _oAuthIntegrationRepository.SaveOAuthIntegrationAsync(integration);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar integração OAuth.");
            return false;
        }
    }

    public async Task<bool> SaveExternalProfileAsync(ProfilesExternal profile)
    {
        try
        {
            return await _oAuthIntegrationRepository.SaveExternalProfileAsync(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar perfil externo.");
            return false;
        }
    }

    public async Task<OAuthIntegration?> GetIntegrationAsync(string provider, int userId)
    {
        try
        {
            return await _oAuthIntegrationRepository.GetIntegrationAsync(provider, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar integração OAuth.");
            return null;
        }
    }

    public async Task<string?> CreateStateAsync(string provider, int userId, TimeSpan lifetime, string? codeVerifier = null)
    {
        try
        {
            var state = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

            var created = await _oAuthIntegrationRepository.CreateStateAsync(new OAuthState
            {
                State = state,
                Provider = provider,
                UserId = userId,
                CodeVerifier = codeVerifier,
                ExpiresAt = DateTime.UtcNow.Add(lifetime)
            });

            return created ? state : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar state OAuth.");
            return null;
        }
    }

    public async Task<OAuthState?> ConsumeStateAsync(string state, string provider)
    {
        try
        {
            return await _oAuthIntegrationRepository.ConsumeStateAsync(state, provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar state OAuth.");
            return null;
        }
    }
}