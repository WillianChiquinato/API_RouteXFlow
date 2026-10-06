using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("profiles_external")]
public class ProfilesExternal : BaseEntity
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("external_user_id")]
    public string ExternalUserId { get; set; } = string.Empty;

    [Column("provider")]
    public string Provider { get; set; } = string.Empty;

    [Column("nickname")]
    public string? Nickname { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("permalink")]
    public string? Permalink { get; set; }

    [Column("country_id")]
    public string? CountryId { get; set; }

    [Column("site_id")]
    public string? SiteId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}