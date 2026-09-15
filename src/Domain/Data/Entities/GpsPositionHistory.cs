using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("gps_position")]
public class GpsPositionHistory : BaseEntity
{
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