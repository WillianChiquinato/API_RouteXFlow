using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_RouteXFlow.Domain.Data.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TypePosition
{
    StartPosition = 1,
    RoutePosition = 2,
    FinishedPosition = 3
}

[Table("gps_position")]
public class GpsPositionHistory : BaseEntity
{
    [Column("typePosition")]
    public TypePosition TypePosition { get; set; }

    [Column("latitude")]
    public string Latitude { get; set; } = string.Empty;

    [Column("longitude")]
    public string Longitude { get; set; } = string.Empty;

    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    [Column("address")]
    public string Address { get; set; } = string.Empty;
}