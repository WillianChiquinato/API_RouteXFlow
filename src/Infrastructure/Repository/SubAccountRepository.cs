using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API_RouteXFlow.Repository;

public class SubAccountRepository : ISubAccountRepository
{
    private readonly AppDbContext _dbContext;

    public SubAccountRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SubAccount>> GetByOwnerAsync(int ownerUserId)
    {
        return await _dbContext.SubAccounts
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<SubAccount?> GetByIdAsync(int id, int ownerUserId)
    {
        return await _dbContext.SubAccounts
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnerUserId == ownerUserId);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbContext.SubAccounts.AnyAsync(x => x.Username == username);
    }

    public async Task<SubAccount?> CreateAsync(SubAccount subAccount)
    {
        _dbContext.SubAccounts.Add(subAccount);

        try
        {
            await _dbContext.SaveChangesAsync();
            return subAccount;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Duas requisições simultâneas com o mesmo nome: o índice único garante que só uma passa.
            return null;
        }
    }

    public async Task<bool> DeleteAsync(SubAccount subAccount)
    {
        _dbContext.SubAccounts.Remove(subAccount);
        return await _dbContext.SaveChangesAsync() > 0;
    }
}
