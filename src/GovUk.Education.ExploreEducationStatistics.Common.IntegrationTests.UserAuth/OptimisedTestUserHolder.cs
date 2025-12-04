using System.Security.Claims;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;

/// <summary>
///
/// This class holds a user that will be available during the processing of an HTTP request.
///
/// This user can be set by using <see cref="SetUser"/> and will be looked up by <see cref="OptimisedTestAuthHandler"/>.
///
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class OptimisedTestUserHolder
{
    private ClaimsPrincipal? _user;

    public void SetUser(ClaimsPrincipal user)
    {
        _user = user;
    }

    public ClaimsPrincipal? GetUser()
    {
        return _user;
    }
}
