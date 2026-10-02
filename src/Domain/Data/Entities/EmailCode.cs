using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_RouteXFlow.Domain.Data.Entities;

public static class EmailCodePurpose
{
    public const string PasswordReset = "password_reset";
    public const string EmailVerification = "email_verification";
}

[Table("email_codes")]
public class EmailCode : BaseEntity
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("purpose")]
    public string Purpose { get; set; } = EmailCodePurpose.PasswordReset;

    [Column("attempts")]
    public int Attempts { get; set; }

    [Column("expiration_time")]
    public DateTime ExpirationTime { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}
