using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("email_codes")]
public class EmailCode : BaseEntity
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("expiration_time")]
    public DateTime ExpirationTime { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}