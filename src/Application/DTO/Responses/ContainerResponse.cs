using API_RouteXFlow.Domain.Data.Entities;

public class ContainerToDevicesDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<Device> Devices { get; set; } = new List<Device>();
}