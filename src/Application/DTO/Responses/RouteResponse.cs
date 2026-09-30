public class RoutePointResponse
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TypePosition { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}

public class RouteSummaryResponse
{
    public int Id { get; set; }
    public int WorkSessionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? OriginAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public decimal TotalMinutes { get; set; }
    public int StopsCount { get; set; }
    public int StopsCompletedCount { get; set; }
}

public class RouteStopResponse
{
    public int Id { get; set; }
    public int Sequence { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AddressNumber { get; set; } = string.Empty;
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public DateTime? EstimatedArrivalAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? AppName { get; set; }
    public string? Notes { get; set; }
    public int? DeliveryOfferId { get; set; }
    public decimal? Value { get; set; }
    public string? TrackingCode { get; set; }
    public string? RecipientName { get; set; }
    public bool? SupplierConfirmed { get; set; }
}

public class RouteDetailResponse : RouteSummaryResponse
{
    public RoutePointResponse Origin { get; set; } = new RoutePointResponse();
    public RoutePointResponse? Destination { get; set; }
    public List<RoutePointResponse> Path { get; set; } = new List<RoutePointResponse>();
    public List<RouteStopResponse> Stops { get; set; } = new List<RouteStopResponse>();
    public bool? Optimized { get; set; }
    public int? ReoptimizationCount { get; set; }
    public decimal? PlannedDistanceKm { get; set; }
    public decimal? PlannedMinutes { get; set; }
}
