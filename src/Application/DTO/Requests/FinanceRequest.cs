using API_RouteXFlow.Domain.Data.Entities;

public class FinanceEntryRegisterRequest
{
    public FinanceEntryType Type { get; set; }
    public FinanceCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class FinanceEntryUpdateRequest : FinanceEntryRegisterRequest
{
    public int Id { get; set; }
}

public class FinanceFilterRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Search { get; set; }
    public FinanceEntryType? Type { get; set; }
}

public class FinanceMonthPeriodRequest
{
    public int Month { get; set; }
    public int Year { get; set; }
}
