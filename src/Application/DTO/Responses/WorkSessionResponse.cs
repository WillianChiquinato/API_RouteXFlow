public class WorkSessionSummaryResponse
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? ContainerId { get; set; }
    public string ContainerName { get; set; } = string.Empty;
    public int DeliveryOffersCount { get; set; }
}

public class WorkSessionDetailResponse : WorkSessionSummaryResponse
{
    public List<DeliveryOfferResponse> DeliveryOffers { get; set; } = new List<DeliveryOfferResponse>();
}

public class DeliveryOfferResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal BonusValue { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public decimal EstimatedMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public List<DeliveryStopResponse> Stops { get; set; } = new List<DeliveryStopResponse>();
    public RouteEvaluationResponse? RouteEvaluation { get; set; }
}

public class DeliveryStopResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
}

public class RouteEvaluationResponse
{
    public bool Recommended { get; set; }
    public decimal AdditionalDistanceKm { get; set; }
    public decimal AdditionalTimeMinutes { get; set; }
    public decimal RouteDeviationKm { get; set; }
    public decimal ValuePerKm { get; set; }
    public int EvaluationScore { get; set; }
    public string Comments { get; set; } = string.Empty;
}
