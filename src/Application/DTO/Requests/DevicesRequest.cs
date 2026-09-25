public class AddDeviceRequest
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Type { get; set; } = 0;
}

public class DeviceRegisterRequest : AddDeviceRequest
{
    public int ContainerIdVinculated { get; set; }
}

public class DeviceEditRequest : AddDeviceRequest
{
    public int Id { get; set; }
}