namespace API_RouteXFlow.Interfaces.Services;

public interface IAiService
{
    Task<string> AnalyzeReportAsync(string reportText, CancellationToken cancellationToken = default);
}
