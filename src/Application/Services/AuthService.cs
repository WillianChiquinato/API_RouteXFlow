using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using BCrypt.Net;

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
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return CustomResponse<string>.Fail("Usuário ou senha inválidos.");
            }

            bool validatePassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!validatePassword)
            {
                return CustomResponse<string>.Fail("E-mail ou senha inválidos.");
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

    public async Task<CustomResponse<string>> RefreshAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return CustomResponse<string>.Fail("Token não informado.");

        var user = _tokenService.GetUserFromToken(token, validateLifetime: false);
        if (user == null)
            return CustomResponse<string>.Fail("Token inválido.");

        var refreshedToken = await _tokenService.GenerateTokenAsync(user);
        return CustomResponse<string>.SuccessTrade(refreshedToken);
    }

    public async Task<CustomResponse<User>> SearchUserByIdAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return CustomResponse<User>.Fail("Usuário nao identificado.");
            }
            
            return CustomResponse<User>.SuccessTrade(user);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}