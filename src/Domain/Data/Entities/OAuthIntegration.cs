using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("oauth_integration")]
public class OAuthIntegration : BaseEntity
{
    [Column("provider")]
    public string Provider { get; set; } = string.Empty;

    [Column("provider_app_id")]
    public int ProviderAppId { get; set; }

    [Column("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [Column("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [Column("expires_in")]
    public int ExpiresIn { get; set; }

    [Column("scope")]
    public string Scope { get; set; } = string.Empty;

    [Column("refresh_token")]
    public string? RefreshToken { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    // Id do usuário no provedor (seller id do Mercado Livre).
    [Column("external_user_id")]
    public long ExternalUserId { get; set; }

    [Column("raw_data")]
    public string RawData { get; set; } = string.Empty;

    [ForeignKey(nameof(ProviderAppId))]
    public Apps? ProviderApp { get; set; }
}