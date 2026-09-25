using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContainerController : ControllerBase
{
    private readonly IContainerService _containerService;

    public ContainerController(IContainerService containerService)
    {
        _containerService = containerService;
    }

    [HttpGet]
    [Route("getContainers")]
    public async Task<IActionResult> GetContainers()
    {
        var containers = await _containerService.GetContainers();

        return containers.Result != null ? Ok(containers) : BadRequest(containers);
    }

    [HttpPost]
    [Route("registerContainer")]
    public async Task<IActionResult> Register(ContainerRegisterRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var container = await _containerService.RegisterAsync(request, int.Parse(userId));

        return container.Success ? Ok(container) : BadRequest(container);
    }


    [HttpPut]
    [Route("updateContainer")]
    public async Task<IActionResult> UpdateContainer(ContainerEditRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }
        
        var editContainer = await _containerService.EditAsync(request, int.Parse(userId));

        return editContainer.Success ? Ok(editContainer) : BadRequest(editContainer);
    }
}
