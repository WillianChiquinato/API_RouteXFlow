using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("delivery_offer")]
public class DeliveryOffers : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("app_id")]
    public int AppId { get; set; }

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("value")]
    public decimal Value { get; set; }

    [Column("bonus_value")]
    public decimal BonusValue { get; set; }

    [Column("total_distance_km")]
    public decimal TotalDistanceKm { get; set; }

    [Column("estimated_minutes")]
    public decimal EstimatedMinutes { get; set; }

    [Column("status_id")]
    public int StatusId { get; set; }

    [Column("worksession_id")]
    public int WorkSessionId { get; set; }

    [Column("detected_at")]
    public DateTime DetectedAt { get; set; }

    [Column("raw_data")]
    public string RawData { get; set; } = string.Empty;

    [ForeignKey(nameof(WorkSessionId))]
    public WorkSession? WorkSession { get; set; }

    [ForeignKey(nameof(AppId))]
    public Apps? Apps { get; set; }

    [ForeignKey(nameof(StatusId))]
    public Status? Status { get; set; }
}