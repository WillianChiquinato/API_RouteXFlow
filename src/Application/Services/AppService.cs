using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class AppService : IAppService
{
    private readonly IAppRepository _appRepository;
    private readonly ILogger<AppService> _logger;

    public AppService(IAppRepository appRepository, ILogger<AppService> logger)
    {
        _appRepository = appRepository;
        _logger = logger;
    }

    public async Task<CustomResponse<List<Apps>>> GetAllAsync()
    {
        try
        {
            var apps = await _appRepository.GetApps();
            
            if (!apps.Any())
            {
                _logger.LogError("Nenhum App encontrado no GetAppss");
                return new CustomResponse<List<Apps>>(false, new List<string> { "Nenhum App encontrado." }, new List<Apps>());
            }
            
            return new CustomResponse<List<Apps>>(true, new List<string>(), apps);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<CustomResponse<Apps>> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
}