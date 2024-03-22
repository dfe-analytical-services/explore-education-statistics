using System;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;

public interface ITokenService
{
    string? GetEmailFromToken(string authToken);

    string GenerateToken(string email, DateTime expiryDateTime);
}
