using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_RouteXFlow.Domain.Data.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType
{
    DeliveryAccount = 1,
    MarketPlaceAccount = 2,
    Guest = 3
}

[Table("user")]
public class User : BaseEntity
{
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [Column("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Column("account_type")]
    public AccountType AccountType { get; set; }

    [Column("email_verified")]
    public bool EmailVerified { get; set; }

    [Column("email_verified_at")]
    public DateTime? EmailVerifiedAt { get; set; }

    [Column("authorization_account_verify")]
    public bool AuthorizationAccountVerify { get; set; }

    [Column("authorization_rejected_at")]
    public DateTime? AuthorizationRejectedAt { get; set; }

    [Column("authorization_rejection_reason")]
    public string? AuthorizationRejectionReason { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }
    
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public ICollection<Preferences> Preferences { get; set; } = new List<Preferences>();
}