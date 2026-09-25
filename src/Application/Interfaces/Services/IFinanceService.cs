using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IFinanceService
{
    Task<CustomResponse<List<FinanceEntry>>> GetEntries(FinanceFilterRequest filter, int userId);
    Task<CustomResponse<FinanceSummaryResponse>> GetSummary(FinanceFilterRequest filter, int userId);
    Task<CustomResponse<FinanceEntry>> RegisterEntry(FinanceEntryRegisterRequest request, int userId);
    Task<CustomResponse<FinanceEntry>> UpdateEntry(FinanceEntryUpdateRequest request, int userId);
    Task<CustomResponse<bool>> DeleteEntry(int id, int userId);
    Task<CustomResponse<FinanceMonthClosure?>> GetMonthClosure(FinanceMonthPeriodRequest period, int userId);
    Task<CustomResponse<FinanceMonthClosure>> CloseMonth(FinanceMonthPeriodRequest period, int userId);
    Task<CustomResponse<FinanceAiReportResponse>> GenerateAiReport(FinanceMonthPeriodRequest period, int userId);
}
