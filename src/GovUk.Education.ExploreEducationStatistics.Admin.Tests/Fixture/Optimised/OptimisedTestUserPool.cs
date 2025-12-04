#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;

/// <summary>
///
/// This class holds a pool of users that can be used in tandem with the <see cref="OptimisedTestAuthHandler"/> to
/// allow HTTP requests to be issued by particular users via HttpClients, or directly via setting the
/// <see cref="OptimisedTestAuthHandler.TestUserId"/> HTTP header.
///
/// Users can be added to the pool by using <see cref="AddUserIfNotExists"/> and looked up using <see cref="GetUser"/>.
///
/// For convenience, a number of common user types are automatically registered with this pool. These are available in
/// <see cref="OptimisedTestUsers"/>.
///
/// </summary>
public class OptimisedTestUserPool
{
    private readonly Dictionary<Guid, ClaimsPrincipal> _users = new();

    public OptimisedTestUserPool()
    {
        AddUserIfNotExists(OptimisedTestUsers.Authenticated);
        AddUserIfNotExists(OptimisedTestUsers.Bau);
        AddUserIfNotExists(OptimisedTestUsers.Analyst);
        AddUserIfNotExists(OptimisedTestUsers.PreReleaseUser);
        AddUserIfNotExists(OptimisedTestUsers.Verified);
        AddUserIfNotExists(OptimisedTestUsers.VerifiedButNotAuthorized);
    }

    public void AddUserIfNotExists(ClaimsPrincipal user)
    {
        if (!_users.ContainsKey(user.GetUserId()))
        {
            _users.Add(user.GetUserId(), user);
        }
    }

    public ClaimsPrincipal? GetUser(Guid id)
    {
        return _users.GetValueOrDefault(id);
    }
}

public static class OptimisedTestUsers
{
    public static ClaimsPrincipal Bau = new DataFixture().BauUser().Generate();

    public static ClaimsPrincipal Analyst = new DataFixture().AnalystUser().Generate();

    public static ClaimsPrincipal Authenticated = new DataFixture().AuthenticatedUser().Generate();

    public static ClaimsPrincipal Verified = new DataFixture().VerifiedByIdentityProviderUser().Generate();

    public static ClaimsPrincipal VerifiedButNotAuthorized = new DataFixture()
        .VerifiedButNotAuthorizedByIdentityProviderUser()
        .Generate();

    public static ClaimsPrincipal PreReleaseUser = new DataFixture().PreReleaseUser().Generate();
}
