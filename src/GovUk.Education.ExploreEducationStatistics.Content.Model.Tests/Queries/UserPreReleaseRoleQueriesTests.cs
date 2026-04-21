using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Queries;

public class UserPreReleaseRoleQueriesTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public void WhereForUser()
    {
        var (user1, user2) = _fixture.DefaultUser().GenerateTuple2();
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userPreReleaseRoles = _fixture
            .DefaultUserPreReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetUser(user1))
            .ForIndex(1, s => s.SetUser(user1))
            // This one should be filtered out
            .ForIndex(2, s => s.SetUser(user2))
            .GenerateList(3);

        var result = userPreReleaseRoles.AsQueryable().WhereForUser(user1.Id).ToList();

        Assert.Equal([userPreReleaseRoles[0], userPreReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereForReleaseVersion()
    {
        User user = _fixture.DefaultUser();
        var (releaseVersion1, releaseVersion2) = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
            .GenerateTuple2();

        var userPreReleaseRoles = _fixture
            .DefaultUserPreReleaseRole()
            .WithUser(user)
            .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
            .ForIndex(1, s => s.SetReleaseVersion(releaseVersion1))
            // This one should be filtered out
            .ForIndex(2, s => s.SetReleaseVersion(releaseVersion2))
            .GenerateList(3);

        var result = userPreReleaseRoles.AsQueryable().WhereForReleaseVersion(releaseVersion1.Id).ToList();

        Assert.Equal([userPreReleaseRoles[0], userPreReleaseRoles[1]], result);
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

        var userPreReleaseRoles = _fixture
            .DefaultUserPreReleaseRole()
            .WithUser(user)
            .ForIndex(0, s => s.SetReleaseVersion(releaseVersions[0]))
            .ForIndex(1, s => s.SetReleaseVersion(releaseVersions[1]))
            // This one should be filtered out
            .ForIndex(2, s => s.SetReleaseVersion(releaseVersions[2]))
            .GenerateList(3);

        var result = userPreReleaseRoles.AsQueryable().WhereForPublication(publication1.Id).ToList();

        Assert.Equal([userPreReleaseRoles[0], userPreReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereEmailNotSent()
    {
        User user = _fixture.DefaultUser();
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userPreReleaseRoles = _fixture
            .DefaultUserPreReleaseRole()
            .WithUser(user)
            .WithReleaseVersion(releaseVersion)
            // These two should be filtered out
            .ForIndex(0, s => s.SetEmailSent(DateTimeOffset.UtcNow))
            .ForIndex(1, s => s.SetEmailSent(DateTimeOffset.UtcNow))
            // The last one should be returned
            .GenerateList(3);

        var result = userPreReleaseRoles.AsQueryable().WhereEmailNotSent().ToList();

        Assert.Equal([userPreReleaseRoles[2]], result);
    }

    [Fact]
    public void WhereUserIsActive()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userPreReleaseRoles = _fixture
            .DefaultUserPreReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()))
            // These ones should be filtered out
            .ForIndex(2, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            .ForIndex(3, s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()))
            .ForIndex(4, s => s.SetUser(_fixture.DefaultSoftDeletedUser()))
            .GenerateList(5);

        var result = userPreReleaseRoles.AsQueryable().WhereUserIsActive().ToList();

        Assert.Equal([userPreReleaseRoles[0], userPreReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereUserHasPendingInvite()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userPreReleaseRoles = _fixture
            .DefaultUserPreReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            .ForIndex(1, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            // These ones should be filtered out
            .ForIndex(2, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(3, s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()))
            .ForIndex(4, s => s.SetUser(_fixture.DefaultSoftDeletedUser()))
            .GenerateList(5);

        var result = userPreReleaseRoles.AsQueryable().WhereUserHasPendingInvite().ToList();

        Assert.Equal([userPreReleaseRoles[0], userPreReleaseRoles[1]], result);
    }

    [Fact]
    public void WhereUserIsActiveOrHasPendingInvite()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userPreReleaseRoles = _fixture
            .DefaultUserPreReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(2, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            .ForIndex(3, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            // These ones should be filtered out
            .ForIndex(4, s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()))
            .ForIndex(5, s => s.SetUser(_fixture.DefaultSoftDeletedUser()))
            .GenerateList(6);

        var result = userPreReleaseRoles.AsQueryable().WhereUserIsActiveOrHasPendingInvite().ToList();

        Assert.Equal(
            [userPreReleaseRoles[0], userPreReleaseRoles[1], userPreReleaseRoles[2], userPreReleaseRoles[3]],
            result
        );
    }
}
