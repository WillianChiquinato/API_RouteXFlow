using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Emails;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using BCrypt.Net;

namespace API_RouteXFlow.Services;

public class AuthService : IAuthService
{
    private readonly TokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly ISubAccountRepository _subAccountRepository;
    private readonly IEmailService _emailService;
    private readonly IEmailVerificationService _emailVerificationService;

    public AuthService(TokenService tokenService, IUserRepository userRepository, ISubAccountRepository subAccountRepository, IEmailService emailService, IEmailVerificationService emailVerificationService)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
        _subAccountRepository = subAccountRepository;
        _emailService = emailService;
        _emailVerificationService = emailVerificationService;
    }

    public async Task<CustomResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return CustomResponse<string>.Fail("Usuário não encontrado.");
            }

            //Enviar no email o link de redefinição de senha
            var resetToken = Guid.NewGuid().ToString();

            var savedToken = await _userRepository.SavePasswordResetTokenAsync(user.Id, resetToken);
            
            if (!savedToken)
            {
                return CustomResponse<string>.Fail("Não foi possível gerar o token de redefinição de senha.");
            }

            var resetLink = $"{Environment.GetEnvironmentVariable("EMAIL_DOMAIN_PROJECT")}/reset-password?token={resetToken}";
            var emailRequest = new EmailRequest
            {
                To = user.Email,
                Subject = "Redefinição de Senha",
                Body = EmailTemplate.Render("ResetPassword", new Dictionary<string, string>
                {
                    ["UserName"] = user.Username,
                    ["UserEmail"] = user.Email,
                    ["UserCpf"] = EmailTemplate.MaskCpf(user.Cpf),
                    ["ResetLink"] = resetLink,
                    ["Year"] = DateTime.UtcNow.Year.ToString()
                })
            };

            var emailSent = await _emailService.SendEmailAsync(emailRequest);
            if (!emailSent.Success)
            {
                return CustomResponse<string>.Fail("Não foi possível enviar o e-mail de redefinição.");
            }

            return CustomResponse<string>.SuccessTrade("Instruções para redefinir a senha foram enviadas para o seu e-mail.");
        }
        catch (Exception ex)
        {
            return CustomResponse<string>.Fail($"Ocorreu um erro ao processar a solicitação: {ex.Message}");
        }
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

            if (!user.EmailVerified)
            {
                return new CustomResponse<string>(false, new List<string> { "Verifique seu e-mail antes de entrar." }, AuthResultCodes.EmailNotVerified);
            }

            if (!user.AuthorizationAccountVerify)
            {
                var message = user.AuthorizationRejectedAt is null
                    ? "Sua conta ainda não foi aprovada pelo administrador, aguarde a avaliação."
                    : $"Seu cadastro foi recusado. Motivo: {user.AuthorizationRejectionReason}";

                return new CustomResponse<string>(false, new List<string> { message }, AuthResultCodes.NotAuthorized);
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

    public async Task<CustomResponse<string>> LoginInAppsAsync(LoginInAppsRequest request)
    {
        try
        {
            var userSubAccount = await _subAccountRepository.GetSubAccountUserByEmailAsync(request.UserName);

            // O repositório devolve uma SubAccount vazia (Id 0) quando não encontra.
            if (userSubAccount == null || userSubAccount.Id == 0)
            {
                return CustomResponse<string>.Fail("Usuário com sub conta Inválido.");
            }

            bool validatePasswordSubAccount = BCrypt.Net.BCrypt.Verify(request.PasswordApp, userSubAccount.PasswordHash);

            if (!validatePasswordSubAccount)
            {
                return CustomResponse<string>.Fail("Senha inválida para sub contas.");
            }

            var subAccountComposeDTO = new SubAccountComposeDTO
            {
                Id = userSubAccount.Id,
                OwnerUserId = userSubAccount.OwnerUserId,
                OwnerEmail = userSubAccount.OwnerUser?.Email,
                UserName = userSubAccount.Username
            };

            var token = await _tokenService.GenerateTokenInSubAccountAsync(subAccountComposeDTO);
            return CustomResponse<string>.SuccessTrade(token);
        }
        catch (Exception ex)
        {
            return CustomResponse<string>.Fail($"An error occurred during login: {ex.Message}");
        }
    }

    public async Task<CustomResponse<string>> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var verified = await _emailVerificationService.VerifyAsync(request.Email, request.Code);
        if (!verified.Success || verified.Result is null)
            return CustomResponse<string>.Fail(verified.Errors.ToArray());

        var user = verified.Result;
        var token = await _tokenService.GenerateTokenAsync(new UserComposeDTO
        {
            Id = user.Id,
            Name = user.Username,
            Email = user.Email
        });

        return CustomResponse<string>.SuccessTrade(token);
    }

    public async Task<CustomResponse<bool>> ResendVerificationAsync(ResendVerificationRequest request)
    {
        return await _emailVerificationService.ResendCodeAsync(request.Email);
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

    public async Task<CustomResponse<string>> RefreshSubAccountAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return CustomResponse<string>.Fail("Token não informado.");

        var claims = _tokenService.GetSubAccountFromToken(token, validateLifetime: false);
        if (claims == null)
            return CustomResponse<string>.Fail("Token inválido.");

        // Confere no banco: sub conta excluída não consegue renovar o token.
        var subAccount = await _subAccountRepository.GetByIdWithOwnerAsync(claims.Id);
        if (subAccount == null || subAccount.OwnerUserId != claims.OwnerUserId)
            return CustomResponse<string>.Fail("Sub conta não encontrada.");

        var refreshedToken = await _tokenService.GenerateTokenInSubAccountAsync(new SubAccountComposeDTO
        {
            Id = subAccount.Id,
            OwnerUserId = subAccount.OwnerUserId,
            OwnerEmail = subAccount.OwnerUser?.Email,
            UserName = subAccount.Username
        });

        return CustomResponse<string>.SuccessTrade(refreshedToken);
    }

    public async Task<CustomResponse<Application.DTO.Responses.SubAccountMeResponse>> GetSubAccountMeAsync(int subAccountId, int ownerUserId)
    {
        var subAccount = await _subAccountRepository.GetByIdWithOwnerAsync(subAccountId);
        if (subAccount == null || subAccount.OwnerUserId != ownerUserId)
            return CustomResponse<Application.DTO.Responses.SubAccountMeResponse>.Fail("Sub conta não encontrada.");

        return CustomResponse<Application.DTO.Responses.SubAccountMeResponse>.SuccessTrade(new Application.DTO.Responses.SubAccountMeResponse
        {
            Id = subAccount.Id,
            Username = subAccount.Username,
            OwnerUserId = subAccount.OwnerUserId,
            OwnerEmail = subAccount.OwnerUser?.Email,
            CreatedAt = subAccount.CreatedAt
        });
    }

    public async Task<CustomResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var user = await _userRepository.GetUserByResetTokenAsync(request.Token);

            if (user == null)
            {
                return CustomResponse<string>.Fail("Token de redefinição inválido ou expirado.");
            }

            var passwordUpdated = await _userRepository.UpdateUserPasswordAsync(user, request.NewPassword);

            if (!passwordUpdated)
            {
                return CustomResponse<string>.Fail("Não foi possível atualizar a senha.");
            }

            // Invalidate the reset token after successful password reset
            await _userRepository.InvalidateResetTokenAsync(user.Id);

            return CustomResponse<string>.SuccessTrade("Senha redefinida com sucesso.");
        }
        catch (Exception ex)
        {
            return CustomResponse<string>.Fail($"Ocorreu um erro ao redefinir a senha: {ex.Message}");
        }
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

    public async Task<CustomResponse<string>> ValidateResetTokenAsync(string token)
    {
        try
        {
            var user = await _userRepository.GetUserByResetTokenAsync(token);

            if (user == null)
            {
                return CustomResponse<string>.Fail("Token de redefinição inválido ou expirado.");
            }

            return CustomResponse<string>.SuccessTrade("Token válido.");
        }
        catch (Exception ex)
        {
            return CustomResponse<string>.Fail($"Ocorreu um erro ao validar o token: {ex.Message}");
        }
    }
}