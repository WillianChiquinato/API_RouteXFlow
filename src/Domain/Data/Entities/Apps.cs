using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("apps")]
public class Apps : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("icon_url")]
    public string? IconUrl { get; set; }
}