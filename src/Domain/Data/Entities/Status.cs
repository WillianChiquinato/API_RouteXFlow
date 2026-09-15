using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("status")]
public class Status : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;
}