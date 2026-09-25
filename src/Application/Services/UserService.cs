using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Domain.Persistence;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAppRepository _appRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, IAppRepository appRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _appRepository = appRepository;
        _logger = logger;
    }

    public async Task<CustomResponse<bool>> RegisterAsync(UserRegisterRequest userRegisterRequest)
    {
        try
        {
            if (EmailBlocklist.IsBlocked(userRegisterRequest.Email))
            {
                _logger.LogWarning("Tentativa de cadastro com e-mail bloqueado: {Email}", userRegisterRequest.Email);
                return new CustomResponse<bool>(false, new List<string> { "Este e-mail não é permitido." }, false);
            }

            if (!Utils.IsCpfValid(userRegisterRequest.Cpf))
            {
                _logger.LogInformation("CPF inválido");
                return new CustomResponse<bool>(false, new List<string>() { "Cpf não é válido para cadastro" }, false);
            }

            var getUserExistenceResponse = await _userRepository.GetAllUsersAsync(userRegisterRequest.Email);
            if (getUserExistenceResponse.Count > 1)
            {
                _logger.LogError("Ja existe um registro cadastrado com esse Email ou CPF");
                return new CustomResponse<bool>(false, new List<string> { "Já existe um registro cadastrado com esse Email ou CPF." }, false);
            }

            var userRep = await _userRepository.RegisterUserAsync(userRegisterRequest);
            var registerAppsInUser = await _appRepository.RegisterAppsVinculatedUser(userRegisterRequest.AppsActives, userRep);

            if (!registerAppsInUser)
            {
                return new CustomResponse<bool>(false, new List<string>{ "Erro ao vincular Apps" }, false);
            }
            
            return new CustomResponse<bool>(true, new List<string>(), userRep > 0);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao registrar usuário");
            throw;
        }
    }
}