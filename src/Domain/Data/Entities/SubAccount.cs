using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

/// <summary>Conta filial: login por nome de usuário + senha, usada nos celulares da operação.</summary>
[Table("sub_account")]
public class SubAccount : BaseEntity
{
    [Column("owner_user_id")]
    public int OwnerUserId { get; set; }

    // Sempre em minúsculas: a unicidade do nome de usuário não diferencia maiúsculas.
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [ForeignKey(nameof(OwnerUserId))]
    public User? OwnerUser { get; set; }
}
