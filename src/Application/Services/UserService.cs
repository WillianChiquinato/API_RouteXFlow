using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Application.DTO;
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

    private static readonly string[] AllowedDateFormats = { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
    private static readonly string[] AllowedTimeFormats = { "24h", "12h" };
    private static readonly string[] AllowedDistanceUnits = { "km", "mi" };
    private static readonly string[] AllowedFuelUnits = { "l", "gal" };
    private const int MinPasswordLength = 8;

    public async Task<CustomResponse<UserProfileDTO>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        try
        {
            var name = request.Name.Trim();
            var phone = new string(request.PhoneNumber.Where(char.IsDigit).ToArray());

            if (name.Length < 3 || name.Length > 100)
                return CustomResponse<UserProfileDTO>.Fail("Informe um nome entre 3 e 100 caracteres.");

            if (phone.Length is < 10 or > 11)
                return CustomResponse<UserProfileDTO>.Fail("Informe um telefone válido com DDD.");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
                return CustomResponse<UserProfileDTO>.Fail("Usuário não identificado.");

            user.Username = name;
            user.PhoneNumber = phone;

            if (!await _userRepository.UpdateUserAsync(user))
                return CustomResponse<UserProfileDTO>.Fail("Não foi possível salvar o perfil.");

            return CustomResponse<UserProfileDTO>.SuccessTrade(UserProfileDTO.From(user));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao atualizar perfil do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<CustomResponse<UserPreferencesDTO>> UpdatePreferencesAsync(int userId, UpdatePreferencesRequest request)
    {
        try
        {
            var vehicleName = request.VehicleName.Trim();

            if (vehicleName.Length > 60)
                return CustomResponse<UserPreferencesDTO>.Fail("O nome do veículo deve ter até 60 caracteres.");

            if (request.VehicleKmPerLiter is <= 0 or > 100)
                return CustomResponse<UserPreferencesDTO>.Fail("O consumo deve ser maior que zero.");

            if (request.FuelPricePerLiter is <= 0 or > 100)
                return CustomResponse<UserPreferencesDTO>.Fail("O preço do combustível deve ser maior que zero.");

            if (!AllowedDateFormats.Contains(request.DateFormat) ||
                !AllowedTimeFormats.Contains(request.TimeFormat) ||
                !AllowedDistanceUnits.Contains(request.DistanceUnit) ||
                !AllowedFuelUnits.Contains(request.FuelUnit))
                return CustomResponse<UserPreferencesDTO>.Fail("Preferência inválida.");

            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                return CustomResponse<UserPreferencesDTO>.Fail("Fuso horário inválido.");
            }

            var preferenceUser = await _userRepository.GetPreferencesUserByIdAsync(userId)
                                 ?? new Preferences { UserId = userId };

            preferenceUser.VehicleName = vehicleName;
            preferenceUser.VehicleKmPerLiter = request.VehicleKmPerLiter;
            preferenceUser.FuelPricePerLiter = request.FuelPricePerLiter;
            preferenceUser.DateFormat = request.DateFormat;
            preferenceUser.TimeFormat = request.TimeFormat;
            preferenceUser.TimeZone = request.TimeZone;
            preferenceUser.DistanceUnit = request.DistanceUnit;
            preferenceUser.FuelUnit = request.FuelUnit;

            if (!await _userRepository.UpdatePreferenceAsync(preferenceUser))
                return CustomResponse<UserPreferencesDTO>.Fail("Não foi possível salvar as preferências.");

            return CustomResponse<UserPreferencesDTO>.SuccessTrade(UserPreferencesDTO.From(preferenceUser));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao atualizar preferências do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<CustomResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        try
        {
            if (request.NewPassword.Length < MinPasswordLength)
                return CustomResponse<bool>.Fail($"A nova senha deve ter pelo menos {MinPasswordLength} caracteres.");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
                return CustomResponse<bool>.Fail("Usuário não identificado.");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return CustomResponse<bool>.Fail("A senha atual está incorreta.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            if (!await _userRepository.UpdateUserAsync(user))
                return CustomResponse<bool>.Fail("Não foi possível alterar a senha.");

            return CustomResponse<bool>.SuccessTrade(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao alterar senha do usuário {UserId}", userId);
            throw;
        }
    }
}
