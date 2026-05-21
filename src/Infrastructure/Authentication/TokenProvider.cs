using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

internal sealed class TokenProvider(IConfiguration configuration) : ITokenProvider
{
    public string Create(User user, List<string> tenantIdentifiers, bool isSystemAdministrator)
    {
        DateTime utcNow = DateTime.UtcNow;
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        string securityStamp = user.SecurityStamp
            ?? throw new InvalidOperationException(
                $"Cannot create JWT for user '{user.Id}': SecurityStamp is null. " +
                "The user may have been created before the Identity migration. Drop and recreate the database.");

        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Nbf, EpochTime.GetIntDate(utcNow).ToString(CultureInfo.InvariantCulture)),
            new Claim("tenant_ids", JsonSerializer.Serialize(tenantIdentifiers)),
            new Claim("is_system_admin", isSystemAdministrator.ToString().ToUpperInvariant()),
            new Claim("security_stamp", securityStamp)
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = utcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            NotBefore = utcNow,
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();

        string token = handler.CreateToken(tokenDescriptor);

        return token;
    }
}
