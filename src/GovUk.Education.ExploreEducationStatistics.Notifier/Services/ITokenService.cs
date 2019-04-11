using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public interface ITokenService
    {
        string GetEmailFromToken(string authToken, string secretKey, ILogger log);

        string GenerateToken(string secretKey, string email, ILogger log);
    }
}