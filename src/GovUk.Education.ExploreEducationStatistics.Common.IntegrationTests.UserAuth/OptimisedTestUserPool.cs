using System.Security.Claims;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;

/// <summary>
///
/// This class holds a pool of users that can be used in tandem with the <see cref="OptimisedTestAuthHandler.TestUserId"/> to
/// allow HTTP requests to be issued by particular users via HttpClients, or directly via setting the
/// <see cref="OptimisedTestAuthHandler"/> HTTP header.
///
/// Users can be added to the pool by using <see cref="GetUser"/> and looked up using <see cref="OptimisedTestAuthHandler"/>.
///
/// </summary>
public class OptimisedTestUserPool
{
    private readonly Dictionary<Guid, ClaimsPrincipal> _users = new();

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

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirstValue("LocalId")!;
        return Guid.Parse(userIdClaim);
    }
}
