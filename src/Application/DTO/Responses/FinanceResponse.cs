public class FinanceSummaryResponse
{
    public decimal TotalEarnings { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public decimal TotalResgates { get; set; }
    public decimal TotalManual { get; set; }
}

public class FinanceAiReportResponse
{
    public string Summary { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new List<string>();
}
