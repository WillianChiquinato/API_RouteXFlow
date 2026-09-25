using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IFinanceRepository
{
    Task<List<FinanceEntry>> GetEntriesAsync(int userId, FinanceFilterRequest filter);
    Task<FinanceEntry?> GetEntryByIdAsync(int id);
    Task<FinanceEntry> RegisterEntryAsync(FinanceEntry entry);
    Task<bool> UpdateEntryAsync(FinanceEntryUpdateRequest request);
    Task<bool> DeleteEntryAsync(int id);
    Task<FinanceMonthClosure?> GetMonthClosureAsync(int userId, int month, int year);
    Task<FinanceMonthClosure> RegisterMonthClosureAsync(FinanceMonthClosure closure);
}
