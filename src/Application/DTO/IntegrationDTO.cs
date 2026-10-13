using System.Text.Json.Serialization;
using API_RouteXFlow.Domain.Data.Entities;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}

public class MercadoLivreUserResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("site_id")]
    public string? SiteId { get; set; }

    [JsonPropertyName("country_id")]
    public string? CountryId { get; set; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("apps")]
    public Apps apps { get; set; } = new Apps();
}

public class MercadoLivreWebhookRequest
{
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    [JsonPropertyName("application_id")]
    public long ApplicationId { get; set; }

    [JsonPropertyName("sent")]
    public DateTime Sent { get; set; }
}

public class FlexHandshakeNotification
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    [JsonPropertyName("resource")]
    public string? Resource { get; set; }
    
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }
    
    [JsonPropertyName("application_id")]
    public long ApplicationId { get; set; }

    [JsonPropertyName("attempts")]
    public int Attempts { get; set; }

    [JsonPropertyName("sent")]
    public DateTime? Sent { get; set; }

    [JsonPropertyName("received")]
    public DateTime? Received { get; set; }
}

public class ShipmentWebhookDTO
{
    public string? ShipmentId { get; set; }
    public long UserId { get; set; }
    public long ApplicationId { get; set; }
    public DateTime? Sent { get; set; }
    public string? Topic { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public TypeShipment TypeShipment { get; set; } = TypeShipment.MercadoLivre;
    public string? Payload { get; set; }
}