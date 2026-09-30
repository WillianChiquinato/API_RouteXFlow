using API_RouteXFlow.Domain.Data.Entities;

public class GpsRoutesHistory
{
    public int WorkSessionId { get; set; }
    public TypePosition TypePosition { get; set; }
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Address { get; set; } = string.Empty;
    public TypeApps TypeApps { get; set; }
}