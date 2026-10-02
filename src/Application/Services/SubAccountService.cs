using System.Text.RegularExpressions;
using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Application.DTO.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class SubAccountService : ISubAccountService
{
    private const int MinPasswordLength = 8;
    private static readonly Regex UsernamePattern = new("^[a-z0-9._]{3,30}$", RegexOptions.Compiled);

    private readonly ISubAccountRepository _subAccountRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SubAccountService> _logger;

    public SubAccountService(ISubAccountRepository subAccountRepository, IUserRepository userRepository, ILogger<SubAccountService> logger)
    {
        _subAccountRepository = subAccountRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<CustomResponse<List<SubAccountDTO>>> ListAsync(int ownerUserId)
    {
        var accounts = await _subAccountRepository.GetByOwnerAsync(ownerUserId);
        return CustomResponse<List<SubAccountDTO>>.SuccessTrade(accounts.Select(SubAccountDTO.From).ToList());
    }

    public async Task<CustomResponse<SubAccountDTO>> CreateAsync(int ownerUserId, CreateSubAccountRequest request)
    {
        try
        {
            var owner = await _userRepository.GetUserByIdAsync(ownerUserId);
            if (owner is null)
                return CustomResponse<SubAccountDTO>.Fail("Usuário não identificado.");

            if (!owner.EmailVerified)
                return CustomResponse<SubAccountDTO>.Fail("Verifique seu e-mail antes de criar contas filiais.");

            var username = request.Username.Trim().ToLowerInvariant();

            if (!UsernamePattern.IsMatch(username))
                return CustomResponse<SubAccountDTO>.Fail("O nome de usuário deve ter de 3 a 30 caracteres, usando apenas letras, números, ponto e underline.");

            if (request.Password.Length < MinPasswordLength)
                return CustomResponse<SubAccountDTO>.Fail($"A senha deve ter pelo menos {MinPasswordLength} caracteres.");

            const string taken = "Este nome de usuário já está em uso.";

            if (await _subAccountRepository.UsernameExistsAsync(username))
                return CustomResponse<SubAccountDTO>.Fail(taken);

            var created = await _subAccountRepository.CreateAsync(new SubAccount
            {
                OwnerUserId = ownerUserId,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            });

            return created is null
                ? CustomResponse<SubAccountDTO>.Fail(taken)
                : CustomResponse<SubAccountDTO>.SuccessTrade(SubAccountDTO.From(created));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao criar conta filial do usuário {UserId}", ownerUserId);
            throw;
        }
    }

    public async Task<CustomResponse<bool>> DeleteAsync(int ownerUserId, int id)
    {
        var subAccount = await _subAccountRepository.GetByIdAsync(id, ownerUserId);
        if (subAccount is null)
            return CustomResponse<bool>.Fail("Conta filial não encontrada.");

        return await _subAccountRepository.DeleteAsync(subAccount)
            ? CustomResponse<bool>.SuccessTrade(true)
            : CustomResponse<bool>.Fail("Não foi possível remover a conta filial.");
    }
}
