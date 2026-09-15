using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("container_device")]
public class ContainerDevices : BaseEntity
{
    [Column("container_id")]
    public int ContainerId { get; set; }

    [Column("device_id")]
    public int DeviceId { get; set; }

    [Column("paired_at")]
    public DateTime PairedAt { get; set; }

    [Column("last_connected_at")]
    public DateTime LastConnectedAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [ForeignKey(nameof(ContainerId))]
    public Container? Container { get; set; }

    [ForeignKey(nameof(DeviceId))]
    public Device? Device { get; set; }
}