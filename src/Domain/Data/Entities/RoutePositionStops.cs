using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("route_position_stop")]
public class RoutePositionStops : BaseEntity
{
    [Column("route_position_id")]
    public int RoutePositionId { get; set; }

    [ForeignKey(nameof(RoutePositionId))]
    public RoutePosition? RoutePosition { get; set; }

    [Column("gps_position_id")]
    public int GpsPositionId { get; set; }

    [ForeignKey(nameof(GpsPositionId))]
    public GpsPositionHistory? GpsPosition { get; set; }

    [Column("sequence")]
    public int Sequence { get; set; }

    [Column("type")]
    public TypeStops Type { get; set; }
}
