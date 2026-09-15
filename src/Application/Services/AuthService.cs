using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Services;

public class AuthService : IAuthService
{
    private readonly TokenService _tokenService;
    private readonly IUserRepository _userRepository;

    public AuthService(TokenService tokenService, IUserRepository userRepository)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
    }

    public async Task<CustomResponse<string>> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userRepository.GetUserByLoginAsync(request.Username, request.Password);

            if (user == null)
            {
                return CustomResponse<string>.Fail("Usuário ou senha inválidos.");
            }

            var userComposeDTO = new UserComposeDTO
            {
                Id = user.Id,
                Name = user.Username,
                Email = user.Email
            };

            var token = await _tokenService.GenerateTokenAsync(userComposeDTO);
            return CustomResponse<string>.SuccessTrade(token);
        }
        catch (Exception ex)
        {
            return CustomResponse<string>.Fail($"An error occurred during login: {ex.Message}");
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        return true;
    }

    public async Task LogoutAsync()
    {
        // Implement your logout logic here, e.g., invalidate the token, clear cookies, etc.
    }
}