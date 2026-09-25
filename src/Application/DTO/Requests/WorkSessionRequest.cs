public class WorkSessionRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class WorkSessionFilterRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
