using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

public enum TypeApps
{
    Delivery = 1,
    MarketPlace = 2
}

[Table("apps")]
public class Apps : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("icon_url")]
    public string? IconUrl { get; set; }

    [Column("type_app")]
    public TypeApps TypeApps { get; set; }
}