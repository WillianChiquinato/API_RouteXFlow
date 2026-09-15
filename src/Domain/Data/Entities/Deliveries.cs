using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("deliveries")]
public class Deliveries : BaseEntity
{
    [Column("delivery_offer_id")]
    public int DeliveryOfferId { get; set; }

    [Column("accepted_at")]
    public DateTime AcceptedAt { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; }

    [Column("completed_at")]
    public DateTime CompletedAt { get; set; }

    [Column("status_id")]
    public int StatusId { get; set; }

    [Column("actual_distance_km")]
    public decimal ActualDistanceKm { get; set; }

    [Column("actual_duration_minutes")]
    public decimal ActualDurationMinutes { get; set; }

    [ForeignKey(nameof(DeliveryOfferId))]
    public DeliveryOffers? DeliveryOffer { get; set; }

    [ForeignKey(nameof(StatusId))]
    public Status? Status { get; set; }
}