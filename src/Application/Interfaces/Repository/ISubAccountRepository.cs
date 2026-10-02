using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface ISubAccountRepository
{
    Task<List<SubAccount>> GetByOwnerAsync(int ownerUserId);
    Task<SubAccount?> GetByIdAsync(int id, int ownerUserId);
    Task<bool> UsernameExistsAsync(string username);
    Task<SubAccount?> CreateAsync(SubAccount subAccount);
    Task<bool> DeleteAsync(SubAccount subAccount);
}
