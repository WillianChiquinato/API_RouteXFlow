using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("route_position")]
public class RoutePosition : BaseEntity
{
    [Column("type")]
    public TypeApps Type { get; set; }

    [Column("worksession_id")]
    public int WorkSessionId { get; set; }

    [ForeignKey(nameof(WorkSessionId))]
    public WorkSession? WorkSession { get; set; }

    [Column("origin_gps_position_id")]
    public int OriginGpsPositionId { get; set; }

    [ForeignKey(nameof(OriginGpsPositionId))]
    public GpsPositionHistory? OriginGpsPosition { get; set; }

    [Column("destination_gps_position_id")]
    public int? DestinationGpsPositionId { get; set; }

    [ForeignKey(nameof(DestinationGpsPositionId))]
    public GpsPositionHistory? DestinationGpsPosition { get; set; }

    public ICollection<RoutePositionStops> Stops { get; set; } = new List<RoutePositionStops>();
}
