using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Queries;

public class UserReleaseRoleQueriesTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public void WhereForUser()
    {
        var (user1, user2) = _fixture.DefaultUser().GenerateTuple2();
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetUser(user1))
            .ForIndex(1, s => s.SetUser(user1))
            // This one should be filtered out
            .ForIndex(2, s => s.SetUser(user2))
            .GenerateList(3);

        var result = userReleaseRoles.AsQueryable().WhereForUser(user1.Id).ToList();

        Assert.Equal([userReleaseRoles[0], userReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereForReleaseVersion()
    {
        User user = _fixture.DefaultUser();
        var (releaseVersion1, releaseVersion2) = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
            .GenerateTuple2();

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
            .ForIndex(1, s => s.SetReleaseVersion(releaseVersion1))
            // This one should be filtered out
            .ForIndex(2, s => s.SetReleaseVersion(releaseVersion2))
            .GenerateList(3);

        var result = userReleaseRoles.AsQueryable().WhereForReleaseVersion(releaseVersion1.Id).ToList();

        Assert.Equal([userReleaseRoles[0], userReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereForPublication()
    {
        User user = _fixture.DefaultUser();
        var (publication1, publication2) = _fixture.DefaultPublication().GenerateTuple2();
        var releaseVersions = _fixture
            .DefaultReleaseVersion()
            .ForIndex(0, s => s.SetRelease(_fixture.DefaultRelease().WithPublication(publication1)))
            .ForIndex(1, s => s.SetRelease(_fixture.DefaultRelease().WithPublication(publication1)))
            .ForIndex(2, s => s.SetRelease(_fixture.DefaultRelease().WithPublication(publication2)))
            .GenerateList(3);

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .ForIndex(0, s => s.SetReleaseVersion(releaseVersions[0]))
            .ForIndex(1, s => s.SetReleaseVersion(releaseVersions[1]))
            // This one should be filtered out
            .ForIndex(2, s => s.SetReleaseVersion(releaseVersions[2]))
            .GenerateList(3);

        var result = userReleaseRoles.AsQueryable().WhereForPublication(publication1.Id).ToList();

        Assert.Equal([userReleaseRoles[0], userReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereRolesIn_EmptyRoles_Throws()
    {
        var userReleaseRoles = Array.Empty<UserReleaseRole>();

        Assert.Throws<ArgumentException>(() => userReleaseRoles.AsQueryable().WhereRolesIn([]));
    }

    [Fact]
    public void WhereRolesIn_NullRoles_Throws()
    {
        var userReleaseRoles = Array.Empty<UserReleaseRole>();

        Assert.Throws<ArgumentException>(() => userReleaseRoles.AsQueryable().WhereRolesIn(null));
    }

    [Fact]
    public void WhereRolesIn_OneRole()
    {
        User user = _fixture.DefaultUser();
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
        ReleaseRole[] rolesToInclude = [ReleaseRole.Approver];

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetRole(ReleaseRole.Approver))
            // These ones should be filtered out
            .ForIndex(1, s => s.SetRole(ReleaseRole.PrereleaseViewer))
            .ForIndex(2, s => s.SetRole(ReleaseRole.Contributor))
            .GenerateList(3);

        var result = userReleaseRoles.AsQueryable().WhereRolesIn(rolesToInclude).ToList();

        Assert.Equal([userReleaseRoles[0]], result);
    }

    [Fact]
    public void WhereRolesIn_MultipleRoles()
    {
        User user = _fixture.DefaultUser();
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
        ReleaseRole[] rolesToInclude = [ReleaseRole.Approver, ReleaseRole.Contributor];

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetRole(ReleaseRole.Approver))
            .ForIndex(1, s => s.SetRole(ReleaseRole.Contributor))
            // This one should be filtered out
            .ForIndex(2, s => s.SetRole(ReleaseRole.PrereleaseViewer))
            .GenerateList(3);

        var result = userReleaseRoles.AsQueryable().WhereRolesIn(rolesToInclude).ToList();

        Assert.Equal([userReleaseRoles[0], userReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereRolesNotIn_EmptyRoles_Throws()
    {
        var userReleaseRoles = Array.Empty<UserReleaseRole>();

        Assert.Throws<ArgumentException>(() => userReleaseRoles.AsQueryable().WhereRolesNotIn([]));
    }

    [Fact]
    public void WhereRolesNotIn_NullRoles_Throws()
    {
        var userReleaseRoles = Array.Empty<UserReleaseRole>();

        Assert.Throws<ArgumentException>(() => userReleaseRoles.AsQueryable().WhereRolesNotIn(null));
    }

    [Fact]
    public void WhereRolesNotIn_OneRole()
    {
        User user = _fixture.DefaultUser();
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
        ReleaseRole[] rolesToNotInclude = [ReleaseRole.Approver];

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
            .ForIndex(1, s => s.SetRole(ReleaseRole.PrereleaseViewer))
            // This one should be filtered out
            .ForIndex(2, s => s.SetRole(ReleaseRole.Approver))
            .GenerateList(3);

        var result = userReleaseRoles.AsQueryable().WhereRolesNotIn(rolesToNotInclude).ToList();

        Assert.Equal([userReleaseRoles[0], userReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereRolesNotIn_MultipleRoles()
    {
        User user = _fixture.DefaultUser();
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
        ReleaseRole[] rolesToNotInclude = [ReleaseRole.Approver, ReleaseRole.Contributor];

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetRole(ReleaseRole.PrereleaseViewer))
            // Both should be filtered out
            .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
            .ForIndex(2, s => s.SetRole(ReleaseRole.Contributor))
            .GenerateList(3);

        var result = userReleaseRoles.AsQueryable().WhereRolesNotIn(rolesToNotInclude).ToList();

        Assert.Equal([userReleaseRoles[0]], result);
    }

    [Fact]
    public void WhereEmailNotSent()
    {
        User user = _fixture.DefaultUser();
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .WithReleaseVersion(releaseVersion)
            // These two should be filtered out
            .ForIndex(0, s => s.SetEmailSent(DateTimeOffset.UtcNow))
            .ForIndex(1, s => s.SetEmailSent(DateTimeOffset.UtcNow))
            // The last one should be returned
            .GenerateList(3);

        var result = userReleaseRoles.AsQueryable().WhereEmailNotSent().ToList();

        Assert.Equal([userReleaseRoles[2]], result);
    }

    [Fact]
    public void WhereUserIsActive()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()))
            // These ones should be filtered out
            .ForIndex(2, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            .ForIndex(3, s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()))
            .ForIndex(4, s => s.SetUser(_fixture.DefaultSoftDeletedUser()))
            .GenerateList(5);

        var result = userReleaseRoles.AsQueryable().WhereUserIsActive().ToList();

        Assert.Equal([userReleaseRoles[0], userReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereUserHasPendingInvite()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            .ForIndex(1, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            // These ones should be filtered out
            .ForIndex(2, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(3, s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()))
            .ForIndex(4, s => s.SetUser(_fixture.DefaultSoftDeletedUser()))
            .GenerateList(5);

        var result = userReleaseRoles.AsQueryable().WhereUserHasPendingInvite().ToList();

        Assert.Equal([userReleaseRoles[0], userReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereUserIsActiveOrHasPendingInvite()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(2, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            .ForIndex(3, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            // These ones should be filtered out
            .ForIndex(4, s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()))
            .ForIndex(5, s => s.SetUser(_fixture.DefaultSoftDeletedUser()))
            .GenerateList(6);

        var result = userReleaseRoles.AsQueryable().WhereUserIsActiveOrHasPendingInvite().ToList();

        Assert.Equal([userReleaseRoles[0], userReleaseRoles[1], userReleaseRoles[2], userReleaseRoles[3]], result);
    }
}
