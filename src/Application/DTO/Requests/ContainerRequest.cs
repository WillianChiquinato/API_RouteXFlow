public class ContainerRegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public List<AddDeviceRequest> Devices { get; set; } = new List<AddDeviceRequest>();
}

public class ContainerEditRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool isActive { get; set; } = false;
}
