using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IRoleRepository roleRepository, ILogger<RoleService> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<CustomResponse<List<Role>>> GetRoles()
    {
        try
        {
            var roles = await _roleRepository.GetRoles();

            if (!roles.Any())
            {
                _logger.LogError("Nenhuma role encontrada no GetRoles");
                return new CustomResponse<List<Role>>(false, new List<string> { "Nenhuma Role encontrada." }, new List<Role>());
            }
            
            return new CustomResponse<List<Role>>(true, new List<string>(), roles);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}