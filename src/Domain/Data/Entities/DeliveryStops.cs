using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

public enum TypeStops
{
    Pickup = 1,
    Delivery = 2
}

[Table("delivery_stop")]
public class DeliveryStops : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("address")]
    public string Address { get; set; } = string.Empty;

    [Column("sequence")]
    public int Sequence { get; set; }

    [Column("type")]
    public TypeStops Type { get; set; }

    [Column("latitude")]
    public string Latitude { get; set; } = string.Empty;

    [Column("longitude")]
    public string Longitude { get; set; } = string.Empty;

    [Column("delivery_offer_id")]
    public int DeliveryOfferId { get; set; }

    [ForeignKey(nameof(DeliveryOfferId))]
    public DeliveryOffers? DeliveryOffer { get; set; }
}