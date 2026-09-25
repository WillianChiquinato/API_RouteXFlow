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
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    [Column("worksession_id")]
    public int WorkSessionId { get; set; }

    [ForeignKey(nameof(WorkSessionId))]
    public WorkSession? WorkSession { get; set; }
}