using System;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public interface ITokenService
    {
        string GetEmailFromToken(string authToken, string secretKey);

        string GenerateToken(string secretKey, string email, DateTime expiryDateTime);
    }
}