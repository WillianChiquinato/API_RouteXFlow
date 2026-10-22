using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace API_RouteXFlow.Services;

public class TokenService
{
    public const string AuthCookieName = "route_x_flow_token";
    public const string SubAccountCookieName = "route_x_flow_sub_token";
    public const string SubAccountName = "sub_account_name";
    public const string SubAccountTypeClaim = "account_type";
    public const string SubAccountTypeValue = "subaccount";
    public const string SubAccountIdClaim = "sub_account_id";
    private const string OAuthStateCookiePrefix = "route_x_flow_oauth_state_";
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> GenerateTokenAsync(UserComposeDTO user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.Name ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(GetJwtSecret())
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetExpireMinutes()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateTokenInSubAccountAsync(SubAccountComposeDTO subAccount)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, subAccount.OwnerUserId.ToString()),
            new Claim(ClaimTypes.Email, subAccount.OwnerEmail ?? string.Empty),
            new Claim(SubAccountName, subAccount.UserName ?? string.Empty),
            new Claim(SubAccountTypeClaim, SubAccountTypeValue),
            new Claim(SubAccountIdClaim, subAccount.Id.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(GetJwtSecret())
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetExpireMinutes()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public void AppendAuthCookie(HttpResponse response, string token, bool isHttps)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(
            GetExpireMinutes()
        );

        response.Cookies.Append(AuthCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = expiresAt,
            Path = "/"
        });
    }

    // Cookie próprio da sub conta: dono (admin) e sub conta (app) logados no mesmo navegador não se sobrescrevem.
    public void AppendSubAccountAuthCookie(HttpResponse response, string token, bool isHttps)
    {
        response.Cookies.Append(SubAccountCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(GetExpireMinutes()),
            Path = "/"
        });
    }

    public void ClearSubAccountAuthCookie(HttpResponse response, bool isHttps)
    {
        response.Cookies.Delete(SubAccountCookieName, BuildCookieOptions(isHttps));
    }

    public void DeleteAuthCookie(HttpResponse response, bool isHttps)
    {
        response.Cookies.Delete(AuthCookieName, BuildCookieOptions(isHttps));
    }

    public void AppendOAuthStateCookie(HttpResponse response, string provider, string state, bool isHttps)
    {
        response.Cookies.Append($"{OAuthStateCookiePrefix}{provider}", state, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(10),
            Path = "/"
        });
    }

    public void DeleteOAuthStateCookie(HttpResponse response, string provider, bool isHttps)
    {
        response.Cookies.Delete($"{OAuthStateCookiePrefix}{provider}", BuildCookieOptions(isHttps));
    }

    private static CookieOptions BuildCookieOptions(bool isHttps)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/"
        };
    }

    public void ClearAuthCookie(HttpResponse response, bool isHttps)
    {
        response.Cookies.Append(AuthCookieName, string.Empty, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            Path = "/"
        });
    }

    public UserComposeDTO? GetUserFromToken(string token, bool validateLifetime = true)
    {
        var jwtToken = ValidateJwt(token, validateLifetime);
        if (jwtToken == null)
            return null;

        // Token de sub conta não pode ser renovado como token de usuário (viraria o dono).
        if (IsSubAccountToken(jwtToken))
            return null;

        var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userId, out var parsedUserId))
            return null;

        return new UserComposeDTO
        {
            Id = parsedUserId,
            Name = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value,
            Email = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value
        };
    }

    public SubAccountComposeDTO? GetSubAccountFromToken(string token, bool validateLifetime = true)
    {
        var jwtToken = ValidateJwt(token, validateLifetime);
        if (jwtToken == null || !IsSubAccountToken(jwtToken))
            return null;

        var ownerId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        var subAccountId = jwtToken.Claims.FirstOrDefault(x => x.Type == SubAccountIdClaim)?.Value;
        if (!int.TryParse(ownerId, out var parsedOwnerId) || !int.TryParse(subAccountId, out var parsedSubAccountId))
            return null;

        return new SubAccountComposeDTO
        {
            Id = parsedSubAccountId,
            OwnerUserId = parsedOwnerId,
            OwnerEmail = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value,
            UserName = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value
        };
    }

    private static bool IsSubAccountToken(JwtSecurityToken jwtToken)
    {
        return jwtToken.Claims.Any(x => x.Type == SubAccountTypeClaim && x.Value == SubAccountTypeValue);
    }

    private JwtSecurityToken? ValidateJwt(string token, bool validateLifetime)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(GetJwtSecret());

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = validateLifetime,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out SecurityToken validatedToken);

            return (JwtSecurityToken)validatedToken;
        }
        catch
        {
            return null;
        }
    }

    private string GetJwtSecret()
    {
        return _config["RouteXFlow:JwtSecret"]
            ?? Environment.GetEnvironmentVariable("ROUTE_X_FLOW_JWT_SECRET")
            ?? throw new InvalidOperationException("Configuracao obrigatoria ausente: RouteXFlow:JwtSecret");
    }

    private double GetExpireMinutes()
    {
        return double.TryParse(_config["RouteXFlow:ExpireMinutes"], out var minutes) && minutes > 0
            ? minutes
            : 60;
    }
}
