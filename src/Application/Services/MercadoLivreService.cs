using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Domain.Persistence;
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

    public async Task<CustomResponse<OAuthIntegration?>> GetIntegrationAsync(int userId)
    {
        var integration = await _oauthIntegrationService.GetIntegrationAsync(Provider, userId);

        if (integration == null)
            return CustomResponse<OAuthIntegration?>.Fail("Integração com o Mercado Livre não encontrada.");

        // Renova o token se estiver expirado (ou prestes a expirar).
        if (integration.ExpiresAt <= DateTime.UtcNow.AddMinutes(1))
        {
            if (!await RefreshTokenAsync(integration))
                return CustomResponse<OAuthIntegration?>.Fail("Sessão do Mercado Livre expirada. Autorize novamente.");
        }

        return CustomResponse<OAuthIntegration?>.SuccessTrade(integration);
    }

    /// <summary>
    /// Envia uma requisição autenticada (Bearer, com refresh automático) ao Mercado Livre.
    /// Retorna null se o usuário não tiver integração válida. O chamador deve descartar a resposta.
    /// </summary>
    public async Task<HttpResponseMessage?> OpenClientAsync(int userId, HttpMethod method, string endpoint, HttpContent? content = null)
    {
        var integrationResponse = await GetIntegrationAsync(userId);

        if (!integrationResponse.Success || integrationResponse.Result == null)
            return null;

        var client = _httpClientFactory.CreateClient("MercadoLivre");
        using var request = new HttpRequestMessage(method, endpoint) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", integrationResponse.Result.AccessToken);

        return await client.SendAsync(request);
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

        var tokenResponse = await response.Content.ReadFromJsonAsync<MercadoLivreTokenResponse>();

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            _logger.LogError("Resposta inválida ao obter token do Mercado Livre.");
            return new CustomResponse<string>(false, new List<string> { "Resposta inválida ao obter token do Mercado Livre." }, null, null);
        }

        if (await _oauthIntegrationService.IsExternalAccountLinkedToOtherUserAsync(Provider, tokenResponse.UserId, oauthState.UserId))
            return CustomResponse<string>.Fail("Esta conta do Mercado Livre já está vinculada a outro usuário.");

        var oauthIntegration = new OAuthIntegration
        {
            Provider = Provider,
            ProviderAppId = 5,
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
        var integration = await GetIntegrationAsync(userId);

        if (!integration.Success || integration.Result == null)
            return CustomResponse<MercadoLivreUserResponse>.Fail(integration.Errors?.FirstOrDefault() ?? "Integração com o Mercado Livre não encontrada.");

        using var response = await OpenClientAsync(userId, HttpMethod.Get, "users/me");

        if (response == null || !response.IsSuccessStatusCode)
            return CustomResponse<MercadoLivreUserResponse>.Fail("Não foi possível consultar o usuário no Mercado Livre.");

        var user = await response.Content.ReadFromJsonAsync<MercadoLivreUserResponse>();

        if (user == null)
            return CustomResponse<MercadoLivreUserResponse>.Fail("Resposta inválida do Mercado Livre.");

        //Tem que ver se a conta do ML é a mesma vinculada ao usuário do RouteXFlow, se não for, não deixa continuar.
        if (user.Id != integration.Result.ExternalUserId)
            return CustomResponse<MercadoLivreUserResponse>.Fail("O usuário do Mercado Livre não corresponde ao usuário autenticado.");

        if (integration.Result.ProviderApp != null)
            user.apps = integration.Result.ProviderApp;

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

        var token = await response.Content.ReadFromJsonAsync<MercadoLivreTokenResponse>();

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

    // O user_id do webhook é o do Mercado Livre (ExternalUserId), então é gravado como veio:
    // a ligação com o usuário do RouteXFlow é feita na leitura, pela integração OAuth.
    public async Task<CustomResponse<string>> HandleWebhookAsync(FlexHandshakeNotification request)
    {
        try
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(request);
            _logger.LogInformation("Webhook do Mercado Livre recebido: {Payload}", payload);

            if (request.Resource == null)
                return CustomResponse<string>.Fail("Requisição de webhook inválida.");

            string? shipmentId = Utils.ExtractShipmentId(request.Resource);

            if (shipmentId is null)
                return CustomResponse<string>.Fail("Não foi possível extrair o ID do envio do recurso.");

            var saved = await _oauthIntegrationService.SaveShipmentWebhookAsync(new ShipmentWebhookDTO
            {
                ShipmentId = shipmentId,
                UserId = request.UserId,
                Topic = request.Topic ?? string.Empty,
                ReceivedAt = DateTime.UtcNow,
                ApplicationId = request.ApplicationId,
                TypeShipment = TypeShipment.MercadoLivre,
                Sent = request.Sent,
                Payload = payload
            });

            if (!saved)
                return CustomResponse<string>.Fail("Erro ao salvar o webhook do Mercado Livre.");

            return CustomResponse<string>.SuccessTrade($"Webhook recebido com sucesso. ID do envio: {shipmentId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook do Mercado Livre.");
            return CustomResponse<string>.Fail("Erro ao processar webhook do Mercado Livre.");
        }
    }

    public async Task<CustomResponse<string>> GetShipmentActiveAsync(int userId)
    {
        var integration = await GetIntegrationAsync(userId);

        if (!integration.Success || integration.Result == null)
            return CustomResponse<string>.Fail(integration.Errors?.FirstOrDefault() ?? "Integração com o Mercado Livre não encontrada.");

        var webhook = await _oauthIntegrationService.GetShipmentIntegrationAsync(integration.Result.ExternalUserId);

        if (webhook == null || string.IsNullOrEmpty(webhook.ShipmentId))
            return CustomResponse<string>.Fail("Nenhum envio ativo encontrado para esta conta do Mercado Livre.");

        // O handshake indica o recurso a consultar (/flex/sites/{SITE}/shipments/{ID}/assignment/v1).
        var assignment = await GetRawAsync(userId, ResolveAssignmentEndpoint(webhook));
        var shipment = await GetRawAsync(userId, $"shipments/{webhook.ShipmentId}");

        if (assignment == null && shipment == null)
            return CustomResponse<string>.Fail("Erro ao consultar o envio no Mercado Livre.");

        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            shipmentId = webhook.ShipmentId,
            assignment = assignment == null ? (System.Text.Json.JsonElement?)null : System.Text.Json.JsonDocument.Parse(assignment).RootElement,
            shipment = shipment == null ? (System.Text.Json.JsonElement?)null : System.Text.Json.JsonDocument.Parse(shipment).RootElement
        });

        return CustomResponse<string>.SuccessTrade(result);
    }

    private static string ResolveAssignmentEndpoint(ShipmentWebhook webhook)
    {
        try
        {
            var resource = System.Text.Json.JsonSerializer.Deserialize<FlexHandshakeNotification>(webhook.Payload)?.Resource;

            if (!string.IsNullOrWhiteSpace(resource) && resource.TrimStart('/').StartsWith("flex/", StringComparison.OrdinalIgnoreCase))
                return resource.TrimStart('/');
        }
        catch (System.Text.Json.JsonException)
        {
            // Payload ilegível: cai no endpoint de envio comum.
        }

        return $"shipments/{webhook.ShipmentId}";
    }

    // Retorna o corpo da resposta, ou null se o Mercado Livre recusar/falhar.
    private async Task<string?> GetRawAsync(int userId, string endpoint)
    {
        using var response = await OpenClientAsync(userId, HttpMethod.Get, endpoint);

        if (response == null)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erro ao consultar {Endpoint} no Mercado Livre ({Status}): {ErrorContent}",
                endpoint, (int)response.StatusCode, await response.Content.ReadAsStringAsync());
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }
}