using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace API_RouteXFlow.Repository;

public class FinanceRepository : IFinanceRepository
{
    private readonly AppDbContext _dbContext;

    public FinanceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<FinanceEntry>> GetEntriesAsync(int userId, FinanceFilterRequest filter)
    {
        var query = _dbContext.FinanceEntries
            .AsNoTracking()
            .Where(entry => entry.UserId == userId);

        if (filter.StartDate.HasValue)
        {
            query = query.Where(entry => entry.Date >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(entry => entry.Date <= filter.EndDate.Value);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(entry => entry.Type == filter.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(entry => entry.Description.Contains(filter.Search));
        }

        return await query.OrderByDescending(entry => entry.Date).ToListAsync();
    }

    public async Task<FinanceEntry?> GetEntryByIdAsync(int id)
    {
        return await _dbContext.FinanceEntries.AsNoTracking().FirstOrDefaultAsync(entry => entry.Id == id);
    }

    public async Task<FinanceEntry> RegisterEntryAsync(FinanceEntry entry)
    {
        _dbContext.Add(entry);
        await _dbContext.SaveChangesAsync();

        return entry;
    }

    public async Task<bool> UpdateEntryAsync(FinanceEntryUpdateRequest request)
    {
        var entry = await _dbContext.FinanceEntries.FirstOrDefaultAsync(e => e.Id == request.Id);

        if (entry is null)
        {
            return false;
        }

        entry.Type = request.Type;
        entry.Category = request.Category;
        entry.Description = request.Description;
        entry.Amount = request.Amount;
        entry.Date = request.Date;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteEntryAsync(int id)
    {
        var entry = await _dbContext.FinanceEntries.FirstOrDefaultAsync(e => e.Id == id);

        if (entry is null)
        {
            return false;
        }

        _dbContext.FinanceEntries.Remove(entry);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<FinanceMonthClosure?> GetMonthClosureAsync(int userId, int month, int year)
    {
        return await _dbContext.FinanceMonthClosures
            .AsNoTracking()
            .FirstOrDefaultAsync(closure => closure.UserId == userId && closure.Month == month && closure.Year == year);
    }

    public async Task<FinanceMonthClosure> RegisterMonthClosureAsync(FinanceMonthClosure closure)
    {
        _dbContext.Add(closure);
        await _dbContext.SaveChangesAsync();

        return closure;
    }
}
