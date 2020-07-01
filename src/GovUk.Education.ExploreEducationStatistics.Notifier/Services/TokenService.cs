using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public class TokenService : ITokenService
    {
        public string GenerateToken(string secretKey, string email, DateTime expiryDateTime)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var secToken = new JwtSecurityToken(
                signingCredentials: credentials,
                issuer: "Sample",
                audience: "Sample",
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Email, email)
                },
                expires: expiryDateTime);

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(secToken);
        }

        public string GetEmailFromToken(string authToken, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters(secretKey);
            string email = null;

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
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)) // The same key as the one that generate the token
            };
        }
    }
}