using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

public enum DeviceType
{
    Manager,
    Navigation,
    Other
}

[Table("device")]
public class Device : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("device_identifier")]
    public string DeviceIdentifier { get; set; } = string.Empty;

    [Column("type")]
    public DeviceType Type { get; set; }
    
    [Column("connected")]
    public bool Connected { get; set; }
}
