using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IRoleRepository
{
    Task<List<Role>> GetRoles();
}