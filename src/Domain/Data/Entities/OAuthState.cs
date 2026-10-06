using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("oauth_states")]
public class OAuthState : BaseEntity
{
    [Column("state")]
    public string State { get; set; } = string.Empty;

    [Column("provider")]
    public string Provider { get; set; } = string.Empty;

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("code_verifier")]
    public string? CodeVerifier { get; set; }

    [Column("Expires_at")]
    public DateTime ExpiresAt { get; set; }
}