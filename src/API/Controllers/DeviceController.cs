using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DeviceController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
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
}