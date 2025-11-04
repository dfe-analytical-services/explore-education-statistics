using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

public class TokenService(IOptions<AppOptions> appOptions) : ITokenService
{
    private readonly AppOptions _appOptions = appOptions.Value;

    public string GenerateToken(string email, DateTime expiryDateTime)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appOptions.TokenSecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var secToken = new JwtSecurityToken(
            signingCredentials: credentials,
            issuer: "Sample",
            audience: "Sample",
            claims: new[] { new Claim(JwtRegisteredClaimNames.Email, email) },
            expires: expiryDateTime
        );

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(secToken);
    }

    public string? GetEmailFromToken(string authToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = GetValidationParameters(_appOptions.TokenSecretKey);
        string? email = null;

        try
        {
            var principal = tokenHandler.ValidateToken(authToken, validationParameters, out var validatedToken);
            if (principal != null)
            {
                email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return email;
    }

    private static TokenValidationParameters GetValidationParameters(string secretKey)
    {
        return new TokenValidationParameters
        {
            ValidateLifetime = true, // Because there is no expiration in the generated token
            ValidateAudience = false, // Because there is no audience in the generated token
            ValidateIssuer = false, // Because there is no issuer in the generated token
            ValidIssuer = "Sample",
            ValidAudience = "Sample",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), // The same key as the one that generate the token
        };
    }
}
