using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Queries;

public class UserPublicationRoleQueriesTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public void WhereForUser()
    {
        var (user1, user2) = _fixture.DefaultUser().GenerateTuple2();
        Publication publication = _fixture.DefaultPublication();

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            .WithPublication(publication)
            .ForIndex(0, s => s.SetUser(user1))
            .ForIndex(1, s => s.SetUser(user1))
            // This one should be filtered out
            .ForIndex(2, s => s.SetUser(user2))
            .GenerateList(3);

        var result = userPublicationRoles.AsQueryable().WhereForUser(user1.Id).ToList();

        Assert.Equal([userPublicationRoles[0], userPublicationRoles[1]], result);
    }

    [Fact]
    public void WhereForPublication()
    {
        User user = _fixture.DefaultUser();
        var (publication1, publication2) = _fixture.DefaultPublication().GenerateTuple2();

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .ForIndex(0, s => s.SetPublication(publication1))
            .ForIndex(1, s => s.SetPublication(publication1))
            // This one should be filtered out
            .ForIndex(2, s => s.SetPublication(publication2))
            .GenerateList(3);

        var result = userPublicationRoles.AsQueryable().WhereForPublication(publication1.Id).ToList();

        Assert.Equal([userPublicationRoles[0], userPublicationRoles[1]], result);
    }

    [Fact]
    public void WhereRolesIn_EmptyRoles_Throws()
    {
        var userPublicationRoles = Array.Empty<UserPublicationRole>();

        Assert.Throws<ArgumentException>(() => userPublicationRoles.AsQueryable().WhereRolesIn([]));
    }

    [Fact]
    public void WhereRolesIn_NullRoles_Throws()
    {
        var userPublicationRoles = Array.Empty<UserPublicationRole>();

        Assert.Throws<ArgumentException>(() => userPublicationRoles.AsQueryable().WhereRolesIn(null));
    }

    [Fact]
    public void WhereRolesIn_OneRole()
    {
        User user = _fixture.DefaultUser();
        Publication publication = _fixture.DefaultPublication();
        PublicationRole[] rolesToInclude = [PublicationRole.Allower];

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            .ForIndex(0, s => s.SetRole(PublicationRole.Allower))
            // This one should be filtered out
            .ForIndex(1, s => s.SetRole(PublicationRole.Owner))
            .GenerateList(2);

        var result = userPublicationRoles.AsQueryable().WhereRolesIn(rolesToInclude).ToList();

        Assert.Equal([userPublicationRoles[0]], result);
    }

    [Fact]
    public void WhereRolesIn_MultipleRoles()
    {
        User user = _fixture.DefaultUser();
        Publication publication = _fixture.DefaultPublication();
        PublicationRole[] rolesToInclude = [PublicationRole.Allower, PublicationRole.Owner];

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            .ForIndex(0, s => s.SetRole(PublicationRole.Allower))
            .ForIndex(1, s => s.SetRole(PublicationRole.Owner))
            .GenerateList(2);

        var result = userPublicationRoles.AsQueryable().WhereRolesIn(rolesToInclude).ToList();

        Assert.Equal([userPublicationRoles[0], userPublicationRoles[1]], result);
    }

    [Fact]
    public void WhereRolesNotIn_EmptyRoles_Throws()
    {
        var userPublicationRoles = Array.Empty<UserPublicationRole>();

        Assert.Throws<ArgumentException>(() => userPublicationRoles.AsQueryable().WhereRolesNotIn([]));
    }

    [Fact]
    public void WhereRolesNotIn_NullRoles_Throws()
    {
        var userPublicationRoles = Array.Empty<UserPublicationRole>();

        Assert.Throws<ArgumentException>(() => userPublicationRoles.AsQueryable().WhereRolesNotIn(null));
    }

    [Fact]
    public void WhereRolesNotIn_OneRole()
    {
        User user = _fixture.DefaultUser();
        Publication publication = _fixture.DefaultPublication();
        PublicationRole[] rolesToNotInclude = [PublicationRole.Allower];

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
            // This one should be filtered out
            .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
            .GenerateList(2);

        var result = userPublicationRoles.AsQueryable().WhereRolesNotIn(rolesToNotInclude).ToList();

        Assert.Equal([userPublicationRoles[0]], result);
    }

    [Fact]
    public void WhereRolesNotIn_MultipleRoles()
    {
        User user = _fixture.DefaultUser();
        Publication publication = _fixture.DefaultPublication();
        PublicationRole[] rolesToNotInclude = [PublicationRole.Allower, PublicationRole.Owner];

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            // Both should be filtered out
            .ForIndex(0, s => s.SetRole(PublicationRole.Allower))
            .ForIndex(1, s => s.SetRole(PublicationRole.Owner))
            .GenerateList(2);

        var result = userPublicationRoles.AsQueryable().WhereRolesNotIn(rolesToNotInclude).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void WhereEmailNotSent()
    {
        User user = _fixture.DefaultUser();
        Publication publication = _fixture.DefaultPublication();

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            // These two should be filtered out
            .ForIndex(0, s => s.SetEmailSent(DateTimeOffset.UtcNow))
            .ForIndex(1, s => s.SetEmailSent(DateTimeOffset.UtcNow))
            // The last one should be returned
            .GenerateList(3);

        var result = userPublicationRoles.AsQueryable().WhereEmailNotSent().ToList();

        Assert.Equal([userPublicationRoles[2]], result);
    }

    [Fact]
    public void WhereUserIsActive()
    {
        Publication publication = _fixture.DefaultPublication();

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            .WithPublication(publication)
            .ForIndex(0, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()))
            // These ones should be filtered out
            .ForIndex(2, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            .ForIndex(3, s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()))
            .ForIndex(4, s => s.SetUser(_fixture.DefaultSoftDeletedUser()))
            .GenerateList(5);

        var result = userPublicationRoles.AsQueryable().WhereUserIsActive().ToList();

        Assert.Equal([userPublicationRoles[0], userPublicationRoles[1]], result);
    }

    [Fact]
    public void WhereUserHasPendingInvite()
    {
        Publication publication = _fixture.DefaultPublication();

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            .WithPublication(publication)
            .ForIndex(0, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            .ForIndex(1, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            // These ones should be filtered out
            .ForIndex(2, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(3, s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()))
            .ForIndex(4, s => s.SetUser(_fixture.DefaultSoftDeletedUser()))
            .GenerateList(5);

        var result = userPublicationRoles.AsQueryable().WhereUserHasPendingInvite().ToList();

        Assert.Equal([userPublicationRoles[0], userPublicationRoles[1]], result);
    }

    [Fact]
    public void WhereUserIsActiveOrHasPendingInvite()
    {
        Publication publication = _fixture.DefaultPublication();

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            .WithPublication(publication)
            .ForIndex(0, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()))
            .ForIndex(2, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            .ForIndex(3, s => s.SetUser(_fixture.DefaultUserWithPendingInvite()))
            // These ones should be filtered out
            .ForIndex(4, s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()))
            .ForIndex(5, s => s.SetUser(_fixture.DefaultSoftDeletedUser()))
            .GenerateList(6);

        var result = userPublicationRoles.AsQueryable().WhereUserIsActiveOrHasPendingInvite().ToList();

        Assert.Equal(
            [userPublicationRoles[0], userPublicationRoles[1], userPublicationRoles[2], userPublicationRoles[3]],
            result
        );
    }
}
