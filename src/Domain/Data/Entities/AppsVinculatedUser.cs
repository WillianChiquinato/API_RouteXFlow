using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("apps_vinculated_user")]
public class AppsVinculatedUser : BaseEntity
{
    [Column("app_id")]
    public int AppId { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
    
    [ForeignKey(nameof(AppId))]
    public Apps? App { get; set; }
}