using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using API_RouteXFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly TokenService _tokenService;

    public DeviceController(IDeviceService deviceService, TokenService tokenService)
    {
        _deviceService = deviceService;
        _tokenService = tokenService;
    }

    [HttpGet]
    [Route("getDevices")]
    public async Task<IActionResult> GetDevices()
    {
        var devices = await _deviceService.GetDevices();
        
        return devices.Result != null ? Ok(devices) : BadRequest(devices);
    }

    [HttpPost]
    [Route("registerDevice")]
    public async Task<IActionResult> RegisterDevice(DeviceRegisterRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var device = await _deviceService.RegisterAsync(request, int.Parse(userId));

        return device.Success ? Ok(device) : BadRequest(device);
    }

    [HttpPut]
    [Route("updateDevice")]
    public async Task<IActionResult> UpdateDevice(DeviceEditRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var device = await _deviceService.EditAsync(request, int.Parse(userId));

        return device.Success ? Ok(device) : BadRequest(device);
    }

    [HttpDelete]
    [Route("deleteDevice/{id:int}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var device = await _deviceService.DeleteAsync(id, int.Parse(userId));

        return device.Success ? Ok(device) : BadRequest(device);
    }

    [HttpPost]
    [Route("sync")]
    public async Task<IActionResult> SyncDevices([FromBody] string codeVerify)
    {
        var subAccount = _tokenService.GetSubAccountFromToken(GetToken());
        if (subAccount == null) return Unauthorized();
        
        var response = await _deviceService.SyncDevices(subAccount.Id, subAccount.OwnerUserId, codeVerify);
        return response.Success ? Ok(response) : Unauthorized(response);
    }
    
    private string GetToken()
    {
        if (Request.Cookies.TryGetValue(TokenService.SubAccountCookieName, out var cookieToken))
            return cookieToken;

        var authorization = Request.Headers.Authorization.ToString();
        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..].Trim()
            : string.Empty;
    }
}