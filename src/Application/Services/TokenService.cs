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

            var jwtToken = (JwtSecurityToken)validatedToken;
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
