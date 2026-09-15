using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("route_evaluation")]
public class RouteEvaluation : BaseEntity
{
    [Column("delivery_offer_id")]
    public int DeliveryOfferId { get; set; }

    [Column("recommended")]
    public bool Recommended { get; set; }

    [Column("additional_distance_km")]
    public decimal AdditionalDistanceKm { get; set; }

    [Column("additional_time_minutes")]
    public decimal AdditionalTimeMinutes { get; set; }

    [Column("route_deviation_km")]
    public decimal RouteDeviationKm { get; set; }

    [Column("value_per_km")]
    public decimal ValuePerKm { get; set; }

    [Column("evaluation_score")]
    public int EvaluationScore { get; set; }

    [Column("comments")]
    public string Comments { get; set; } = string.Empty;

    [ForeignKey(nameof(DeliveryOfferId))]
    public DeliveryOffers? DeliveryOffer { get; set; }
}