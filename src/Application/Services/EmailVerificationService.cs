using System.Security.Cryptography;
using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Emails;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public EmailVerificationService(IUserRepository userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<CustomResponse<bool>> SendCodeAsync(User user)
    {
        var code = RandomNumberGenerator.GetInt32(0, 100_000).ToString("D5");

        if (!await _userRepository.SaveEmailVerificationCodeAsync(user.Id, code))
            return CustomResponse<bool>.Fail("Não foi possível gerar o código de verificação.");

        var sent = await _emailService.SendEmailAsync(new EmailRequest
        {
            To = user.Email,
            Subject = "Seu código de verificação",
            Body = EmailTemplate.Render("VerifyEmail", new Dictionary<string, string>
            {
                ["UserName"] = user.Username,
                ["Code"] = code,
                ["Year"] = DateTime.UtcNow.Year.ToString()
            })
        });

        return sent.Success
            ? CustomResponse<bool>.SuccessTrade(true)
            : CustomResponse<bool>.Fail("Não foi possível enviar o e-mail de verificação.");
    }

    public async Task<CustomResponse<bool>> ResendCodeAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(Normalize(email));

        // Resposta genérica para não revelar quais e-mails existem na base.
        if (user is null || user.EmailVerified)
            return CustomResponse<bool>.SuccessTrade(true);

        var current = await _userRepository.GetEmailVerificationCodeAsync(user.Id);
        if (current is not null && DateTime.UtcNow - current.CreatedAt < ResendCooldown)
            return CustomResponse<bool>.Fail("Aguarde alguns instantes antes de pedir um novo código.");

        return await SendCodeAsync(user);
    }

    public async Task<CustomResponse<User>> VerifyAsync(string email, string code)
    {
        const string invalid = "Código inválido ou expirado.";

        var user = await _userRepository.GetUserByEmailAsync(Normalize(email));
        if (user is null)
            return CustomResponse<User>.Fail(invalid);

        if (user.EmailVerified)
            return CustomResponse<User>.SuccessTrade(user);

        var emailCode = await _userRepository.GetEmailVerificationCodeAsync(user.Id);
        if (emailCode is null || emailCode.ExpirationTime <= DateTime.UtcNow)
            return CustomResponse<User>.Fail(invalid);

        if (emailCode.Attempts >= MaxAttempts)
        {
            await _userRepository.RemoveEmailCodeAsync(emailCode);
            return CustomResponse<User>.Fail("Muitas tentativas. Solicite um novo código.");
        }

        if (!CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(emailCode.Code),
                System.Text.Encoding.UTF8.GetBytes(code.Trim())))
        {
            await _userRepository.RegisterEmailCodeAttemptAsync(emailCode);
            return CustomResponse<User>.Fail(invalid);
        }

        await _userRepository.MarkEmailVerifiedAsync(user.Id);
        await _userRepository.RemoveEmailCodeAsync(emailCode);
        user.EmailVerified = true;

        return CustomResponse<User>.SuccessTrade(user);
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
