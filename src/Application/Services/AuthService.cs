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
    private readonly IEmailService _emailService;

    public AuthService(TokenService tokenService, IUserRepository userRepository, IEmailService emailService)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
        _emailService = emailService;
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

    public async Task<CustomResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var user = await _userRepository.GetUserByResetTokenAsync(request.Token);

            if (user == null)
            {
                return CustomResponse<string>.Fail("Token de redefinição inválido ou expirado.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var passwordUpdated = await _userRepository.UpdateUserPasswordAsync(user, hashedPassword);

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