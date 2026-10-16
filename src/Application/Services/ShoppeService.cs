using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Domain.Persistence;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class ShoppeService : IShoppeService
{
    private readonly ILogger<ShoppeService> _logger;
    private readonly IOAuthIntegrationService _oauthIntegrationService;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string Provider = "Shopee";

    public ShoppeService(ILogger<ShoppeService> logger, IOAuthIntegrationService oauthIntegrationService, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _oauthIntegrationService = oauthIntegrationService;
        _httpClientFactory = httpClientFactory;
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

    public async Task<CustomResponse<string>> GetAuthorizationShoppeAsync(int userId)
    {
        try
        {
            var partnerIdStr = Environment.GetEnvironmentVariable("SHOPPE_PARTNER_ID");
            var partnerKey = Environment.GetEnvironmentVariable("SHOPPE_PARTNER_KEY");
            var baseUri = Environment.GetEnvironmentVariable("SHOPPE_BASE_URI");
            var redirectUri = Environment.GetEnvironmentVariable("SHOPPE_REDIRECT_URI");

            if (!long.TryParse(partnerIdStr, out var partnerId) ||
                string.IsNullOrEmpty(partnerKey) ||
                string.IsNullOrEmpty(baseUri) ||
                string.IsNullOrEmpty(redirectUri))
            {
                return CustomResponse<string>.Fail("Configurações do Shopee não estão definidas.");
            }

            string path = "/shop/auth_partner";
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // 1. Gera a assinatura HMAC-SHA256 específica para o auth.
            string sign = ShopeeAuthHelper.GenerateSign(partnerId, path, timestamp, partnerKey);

            // Shopee não usa PKCE, então o state é gerado sem code_verifier (uso único, expira em 10 min).
            var state = await _oauthIntegrationService.CreateStateAsync(Provider, userId, TimeSpan.FromMinutes(10));

            if (state == null)
                return CustomResponse<string>.Fail("Não foi possível iniciar a autorização.");

            var redirectWithState = $"{redirectUri}{(redirectUri.Contains('?') ? "&" : "?")}state={state}";
            var authorizationUrl = $"{baseUri}{path}?partner_id={partnerId}&timestamp={timestamp}&sign={sign}&redirect={Uri.EscapeDataString(redirectWithState)}";

            return CustomResponse<string>.SuccessTrade(authorizationUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter a URL de autorização do Shopee.");
            return CustomResponse<string>.Fail("Erro ao obter a URL de autorização do Shopee.");
        }
    }

    public async Task<CustomResponse<string>> HandleOAuthCallbackAsync(string code, string shop_id, string partner_id, string state)
    {
        try
        {
            var oauthState = await _oauthIntegrationService.ConsumeStateAsync(state, Provider);

            if (oauthState == null)
                return CustomResponse<string>.Fail("State inválido ou expirado.");

            var partnerKey = Environment.GetEnvironmentVariable("SHOPPE_PARTNER_KEY");
            var baseUri = Environment.GetEnvironmentVariable("SHOPPE_BASE_URI");

            if (string.IsNullOrEmpty(partnerKey) || string.IsNullOrEmpty(baseUri))
            {
                return CustomResponse<string>.Fail("Configurações do Shopee não estão definidas.");
            }

            var client = _httpClientFactory.CreateClient("ShopeeClient");

            // Chama o token.
            // Shoppe para validar precisa tanto de query quanto de body.
            var tokenUrl = $"{baseUri}/auth/token/get";
            var queryString = $"partner_id={partner_id}&timestamp={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}&sign={ShopeeAuthHelper.GenerateSign(long.Parse(partner_id), "/auth/token/get", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), partnerKey)}";

            var requestData = new Dictionary<string, string>
            {
                { "code", code },
                { "shop_id", shop_id },
                { "partner_id", partner_id },
            };

            using var response = await client.PostAsJsonAsync($"{tokenUrl}?{queryString}", requestData);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao obter o token do Shopee: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return CustomResponse<string>.Fail($"Erro ao obter o token do Shopee: {response.StatusCode}");
            }
            var shoppeTokenResponse = await response.Content.ReadFromJsonAsync<ShopeeTokenResponse>();

            if (shoppeTokenResponse == null || string.IsNullOrEmpty(shoppeTokenResponse.AccessToken))
            {
                _logger.LogError("Resposta inválida ao obter o token do Shopee.");
                return CustomResponse<string>.Fail("Resposta inválida ao obter o token do Shopee.");
            }

            if (await _oauthIntegrationService.IsExternalAccountLinkedToOtherUserAsync(Provider, shoppeTokenResponse.ShopId, oauthState.UserId))
                return CustomResponse<string>.Fail("Esta conta do Shopee já está vinculada a outro usuário.");

            var oauthIntegration = new OAuthIntegration
            {
                Provider = Provider,
                UserId = oauthState.UserId,
                AccessToken = shoppeTokenResponse.AccessToken,
                RefreshToken = shoppeTokenResponse.RefreshToken,
                TokenType = "Bearer",
                ExpiresIn = shoppeTokenResponse.ExpiresIn,
                ExpiresAt = DateTime.UtcNow.AddSeconds(shoppeTokenResponse.ExpiresIn),
                Scope = null,
                ExternalUserId = shoppeTokenResponse.ShopId,
                RawData = System.Text.Json.JsonSerializer.Serialize(shoppeTokenResponse)
            };

            var integrationSaved = await _oauthIntegrationService.SaveOAuthIntegrationAsync(oauthIntegration);
            
            if (!integrationSaved)
            {
                _logger.LogError("Erro ao salvar a integração com o Shopee.");
                return CustomResponse<string>.Fail("Erro ao salvar a integração com o Shopee.");
            }

            return CustomResponse<string>.SuccessTrade("Integração com o Shopee realizada com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar o callback OAuth do Shopee.");
            return CustomResponse<string>.Fail("Erro ao processar o callback OAuth do Shopee.");
        }
    }

    public async Task<CustomResponse<ShopeeOrderResponse>> GetOrdersListAsync(int userId)
    {
        try
        {
            var orderStatus = "READY_TO_SHIP";
            var partnerIdStr = Environment.GetEnvironmentVariable("SHOPPE_PARTNER_ID");
            var partnerKey = Environment.GetEnvironmentVariable("SHOPPE_PARTNER_KEY");
            var baseUri = Environment.GetEnvironmentVariable("SHOPPE_BASE_URI");

            long.TryParse(partnerIdStr, out var partnerId);

            if (!long.TryParse(partnerIdStr, out partnerId) ||
                string.IsNullOrEmpty(partnerKey) ||
                string.IsNullOrEmpty(baseUri))
            {
                return CustomResponse<ShopeeOrderResponse>.Fail("Configurações do Shopee não estão definidas.");
            }

            var oauthIntegration = await GetIntegrationAsync(userId);

            if (!oauthIntegration.Success || oauthIntegration.Result == null)
                return CustomResponse<ShopeeOrderResponse>.Fail("Integração com o Shopee não encontrada.");
            
            string path = "/api/v2/order/get_order_list";
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string sign = ShopeeAuthHelper.GenerateSign(partnerId, path, timestamp, partnerKey);

            long timeFrom = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
            long timeTo = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var client = _httpClientFactory.CreateClient("ShopeeClient");

            string url = $"{baseUri}{path}?" +
                 $"partner_id={partnerId}&" +
                 $"timestamp={timestamp}&" +
                 $"access_token={oauthIntegration.Result.AccessToken}&" +
                 $"shop_id={oauthIntegration.Result.ExternalUserId}&" +
                 $"sign={sign}&" +
                 $"time_range_field=create_time&" +
                 $"time_from={timeFrom}&" +
                 $"time_to={timeTo}&" +
                 $"page_size=50&" +
                 $"order_status={orderStatus}";

            using var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            var ordersResponse = JsonSerializer.Deserialize<ShopeeOrderResponse>(content);

            return CustomResponse<ShopeeOrderResponse>.SuccessTrade(ordersResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter a lista de pedidos do Shopee.");
            return CustomResponse<ShopeeOrderResponse>.Fail("Erro ao obter a lista de pedidos do Shopee.");
        }
    }
}