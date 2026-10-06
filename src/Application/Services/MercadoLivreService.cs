using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class MercadoLivreService : IMercadoLivreService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOAuthIntegrationService _oauthIntegrationService;
    private readonly ILogger<MercadoLivreService> _logger;

    public MercadoLivreService(IHttpClientFactory httpClientFactory, IOAuthIntegrationService oauthIntegrationService, ILogger<MercadoLivreService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _oauthIntegrationService = oauthIntegrationService;
        _logger = logger;
    }

    private const string Provider = "MercadoLivre";

    private static string Base64UrlEncoder(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public async Task<CustomResponse<string>> GetAuthorizationUrlAsync(int userId)
    {
        var clientId = Environment.GetEnvironmentVariable("MERCADO_LIVRE_CLIENT_ID")
            ?? throw new InvalidOperationException("Configure a variável de ambiente MERCADO_LIVRE_CLIENT_ID.");
        var redirectUri = Environment.GetEnvironmentVariable("MERCADO_LIVRE_REDIRECT_URI")
            ?? throw new InvalidOperationException("Configure a variável de ambiente MERCADO_LIVRE_REDIRECT_URI.");
        var urlMl = Environment.GetEnvironmentVariable("MERCADO_LIVRE_AUTH_URL")
            ?? throw new InvalidOperationException("Configure a variável de ambiente MERCADO_LIVRE_URL.");

        // PKCE: o verifier fica guardado junto do state; só o challenge (SHA-256) vai na URL.
        var codeVerifier = Base64UrlEncoder(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
        var codeChallenge = Base64UrlEncoder(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(codeVerifier)));

        var state = await _oauthIntegrationService.CreateStateAsync(Provider, userId, TimeSpan.FromMinutes(10), codeVerifier);

        if (state == null)
            return CustomResponse<string>.Fail("Não foi possível iniciar a autorização.");

        var url = $"{urlMl}?response_type=code" +
                  $"&client_id={Uri.EscapeDataString(clientId)}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&state={state}" +
                  $"&code_challenge={codeChallenge}" +
                  "&code_challenge_method=S256";

        return CustomResponse<string>.SuccessTrade(url);
    }

    public async Task<CustomResponse<string>> HandleOAuthCallbackAsync(string code, string state)
    {
        var oauthState = await _oauthIntegrationService.ConsumeStateAsync(state, Provider);

        if (oauthState == null)
            return CustomResponse<string>.Fail("State inválido ou expirado.");

        var clientId = Environment.GetEnvironmentVariable("MERCADO_LIVRE_CLIENT_ID")
            ?? throw new InvalidOperationException("Configure a variável de ambiente MERCADO_LIVRE_CLIENT_ID.");
        var clientSecret = Environment.GetEnvironmentVariable("MERCADO_LIVRE_CLIENT_SECRET")
            ?? throw new InvalidOperationException("Configure a variável de ambiente MERCADO_LIVRE_CLIENT_SECRET.");
        var redirectUri = Environment.GetEnvironmentVariable("MERCADO_LIVRE_REDIRECT_URI")
            ?? throw new InvalidOperationException("Configure a variável de ambiente MERCADO_LIVRE_REDIRECT_URI.");

        var client = _httpClientFactory.CreateClient("MercadoLivre");

        var requestData = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "code_verifier", oauthState.CodeVerifier ?? string.Empty }
        };

        using var response = await client.PostAsync("oauth/token", new FormUrlEncodedContent(requestData));

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro ao obter token do Mercado Livre: {ErrorContent}", errorContent);
            return new CustomResponse<string>(false, new List<string> { "Erro ao obter token do Mercado Livre." }, null, null);
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            _logger.LogError("Resposta inválida ao obter token do Mercado Livre.");
            return new CustomResponse<string>(false, new List<string> { "Resposta inválida ao obter token do Mercado Livre." }, null, null);
        }

        var oauthIntegration = new OAuthIntegration
        {
            Provider = "MercadoLivre",
            AccessToken = tokenResponse.AccessToken,
            TokenType = tokenResponse.TokenType ?? string.Empty,
            ExpiresIn = tokenResponse.ExpiresIn,
            Scope = tokenResponse.Scope ?? string.Empty,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            UserId = oauthState.UserId,
            ExternalUserId = tokenResponse.UserId,
            RawData = string.Empty
        };

        var integrationSaved = await _oauthIntegrationService.SaveOAuthIntegrationAsync(oauthIntegration);

        if (!integrationSaved)
            return CustomResponse<string>.Fail("Erro ao salvar a integração com o Mercado Livre.");

        return CustomResponse<string>.SuccessTrade("Integração com o Mercado Livre realizada com sucesso.");
    }

    public async Task<CustomResponse<MercadoLivreUserResponse>> GetUserInfoAsync(int userId)
    {
        var integration = await _oauthIntegrationService.GetIntegrationAsync(Provider, userId);

        if (integration == null)
            return CustomResponse<MercadoLivreUserResponse>.Fail("Integração com o Mercado Livre não encontrada.");

        // Renova o token se estiver expirado (ou prestes a expirar).
        if (integration.ExpiresAt <= DateTime.UtcNow.AddMinutes(1))
        {
            if (!await RefreshTokenAsync(integration))
                return CustomResponse<MercadoLivreUserResponse>.Fail("Sessão do Mercado Livre expirada. Autorize novamente.");
        }

        var client = _httpClientFactory.CreateClient("MercadoLivre");
        using var request = new HttpRequestMessage(HttpMethod.Get, "users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", integration.AccessToken);

        using var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro ao consultar usuário do Mercado Livre: {ErrorContent}", errorContent);
            return CustomResponse<MercadoLivreUserResponse>.Fail("Erro ao consultar usuário no Mercado Livre.");
        }

        var user = await response.Content.ReadFromJsonAsync<MercadoLivreUserResponse>();

        if (user == null)
            return CustomResponse<MercadoLivreUserResponse>.Fail("Resposta inválida do Mercado Livre.");

        var profileSaved = await _oauthIntegrationService.SaveExternalProfileAsync(new ProfilesExternal
        {
            UserId = userId,
            Provider = Provider,
            ExternalUserId = user.Id.ToString(),
            Nickname = user.Nickname,
            Email = user.Email,
            Permalink = user.Permalink,
            CountryId = user.CountryId,
            SiteId = user.SiteId
        });

        if (!profileSaved)
            return CustomResponse<MercadoLivreUserResponse>.Fail("Erro ao salvar o perfil do Mercado Livre.");

        return CustomResponse<MercadoLivreUserResponse>.SuccessTrade(user);
    }

    private async Task<bool> RefreshTokenAsync(OAuthIntegration integration)
    {
        if (string.IsNullOrEmpty(integration.RefreshToken))
            return false;

        var clientId = Environment.GetEnvironmentVariable("MERCADO_LIVRE_CLIENT_ID")
            ?? throw new InvalidOperationException("Configure a variável de ambiente MERCADO_LIVRE_CLIENT_ID.");
        var clientSecret = Environment.GetEnvironmentVariable("MERCADO_LIVRE_CLIENT_SECRET")
            ?? throw new InvalidOperationException("Configure a variável de ambiente MERCADO_LIVRE_CLIENT_SECRET.");

        var client = _httpClientFactory.CreateClient("MercadoLivre");

        using var response = await client.PostAsync("oauth/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "refresh_token", integration.RefreshToken }
        }));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erro ao renovar token do Mercado Livre: {ErrorContent}", await response.Content.ReadAsStringAsync());
            return false;
        }

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if (token == null || string.IsNullOrEmpty(token.AccessToken))
            return false;

        integration.AccessToken = token.AccessToken;
        // O refresh token do Mercado Livre é de uso único: o novo substitui o anterior.
        integration.RefreshToken = token.RefreshToken ?? integration.RefreshToken;
        integration.TokenType = token.TokenType ?? integration.TokenType;
        integration.ExpiresIn = token.ExpiresIn;
        integration.ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

        return await _oauthIntegrationService.SaveOAuthIntegrationAsync(integration);
    }
}