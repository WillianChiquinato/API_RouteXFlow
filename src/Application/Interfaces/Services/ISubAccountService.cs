using API_RouteXFlow.Responses;
using Application.DTO.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface ISubAccountService
{
    Task<CustomResponse<List<SubAccountDTO>>> ListAsync(int ownerUserId);
    Task<CustomResponse<SubAccountDTO>> CreateAsync(int ownerUserId, CreateSubAccountRequest request);
    Task<CustomResponse<bool>> DeleteAsync(int ownerUserId, int id);
}
