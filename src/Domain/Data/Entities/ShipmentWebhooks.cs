using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

public enum TypeShipment
{
    MercadoLivre = 1,
    Shoppe = 2,
    Other = 3
}

[Table("shipment_webhook")]
public class ShipmentWebhook : BaseEntity
{
    [Column("shipment_id")]
    public string ShipmentId { get; set; } = string.Empty;

    [Column("received_at")]
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("application_id")]
    public long ApplicationId { get; set; }

    [Column("sent")]
    public DateTimeOffset? Sent { get; set; }

    [Column("topic")]
    public string Topic { get; set; } = string.Empty;

    [Column("type_shipment")]
    public TypeShipment TypeShipment { get; set; } = TypeShipment.MercadoLivre;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("payload")]
    public string Payload { get; set; } = string.Empty;
}