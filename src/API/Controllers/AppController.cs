using API_RouteXFlow.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppController : ControllerBase
{
    private readonly IAppService _appService;

    public AppController(IAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    [Route("getApps")]
    [AllowAnonymous]
    public async Task<IActionResult> GetApps()
    {
        var apps = await _appService.GetAllAsync();
        
        return apps.Result != null ? Ok(apps) : NotFound(apps);
    }
}