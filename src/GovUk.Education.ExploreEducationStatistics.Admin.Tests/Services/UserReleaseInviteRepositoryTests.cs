#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserReleaseInviteRepositoryTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task Create()
    {
        var releaseVersionId = Guid.NewGuid();
        var createdById = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = new UserReleaseInviteRepository(contentDbContext);
            await repository.Create(
                releaseVersionId: releaseVersionId,
                email: "test@test.com",
                releaseRole: ReleaseRole.Contributor,
                emailSent: true,
                createdById: createdById);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userReleaseInvite = await contentDbContext.UserReleaseInvites
                .AsQueryable()
                .SingleOrDefaultAsync();

            Assert.NotNull(userReleaseInvite);
            Assert.Equal(releaseVersionId, userReleaseInvite.ReleaseVersionId);
            Assert.Equal("test@test.com", userReleaseInvite.Email);
            Assert.Equal(ReleaseRole.Contributor, userReleaseInvite.Role);
            Assert.True(userReleaseInvite.EmailSent);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvite.Created).Milliseconds, 0, 1500);
            Assert.Equal(createdById, userReleaseInvite.CreatedById);
        }
    }

    [Fact]
    public async Task CreateManyIfNotExists()
    {
        var createdById = Guid.NewGuid();
        var releaseVersionId1 = Guid.NewGuid();
        var releaseVersionId2 = Guid.NewGuid();
        var existingReleaseInvite = new UserReleaseInvite
        {
            Email = "test@test.com",
            ReleaseVersionId = Guid.NewGuid(),
            Role = ReleaseRole.Contributor,
            EmailSent = true,
            CreatedById = createdById,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(existingReleaseInvite);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = new UserReleaseInviteRepository(contentDbContext);
            await repository.CreateManyIfNotExists(
                releaseVersionIds: ListOf(releaseVersionId1, releaseVersionId2,
                    existingReleaseInvite.ReleaseVersionId),
                email: "test@test.com",
                releaseRole: ReleaseRole.Contributor,
                emailSent: false,
                createdById: createdById);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userReleaseInvites = await contentDbContext.UserReleaseInvites
                .AsQueryable()
                .ToListAsync();

            Assert.Equal(3, userReleaseInvites.Count);

            Assert.Equal(existingReleaseInvite.Id, userReleaseInvites[0].Id);
            Assert.Equal(existingReleaseInvite.ReleaseVersionId, userReleaseInvites[0].ReleaseVersionId);
            Assert.Equal("test@test.com", userReleaseInvites[0].Email);
            Assert.Equal(ReleaseRole.Contributor, userReleaseInvites[0].Role);
            Assert.True(userReleaseInvites[0].EmailSent);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[0].Created).Milliseconds, 0, 1500);
            Assert.Equal(createdById, userReleaseInvites[0].CreatedById);

            Assert.Equal(releaseVersionId1, userReleaseInvites[1].ReleaseVersionId);
            Assert.Equal("test@test.com", userReleaseInvites[1].Email);
            Assert.Equal(ReleaseRole.Contributor, userReleaseInvites[1].Role);
            Assert.False(userReleaseInvites[1].EmailSent);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[1].Created).Milliseconds, 0, 1500);
            Assert.Equal(createdById, userReleaseInvites[1].CreatedById);

            Assert.Equal(releaseVersionId2, userReleaseInvites[2].ReleaseVersionId);
            Assert.Equal("test@test.com", userReleaseInvites[2].Email);
            Assert.Equal(ReleaseRole.Contributor, userReleaseInvites[2].Role);
            Assert.False(userReleaseInvites[2].EmailSent);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[2].Created).Milliseconds, 0, 1500);
            Assert.Equal(createdById, userReleaseInvites[2].CreatedById);
        }
    }

    [Fact]
    public async Task UserHasInvite_True()
    {
        var invite = new UserReleaseInvite
        {
            Email = "test@test.com",
            ReleaseVersionId = Guid.NewGuid(),
            Role = ReleaseRole.Contributor,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(invite);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = new UserReleaseInviteRepository(contentDbContext);
            var result = await repository.UserHasInvite(invite.ReleaseVersionId, invite.Email, invite.Role);
            Assert.True(result);
        }
    }

    [Fact]
    public async Task UserHasInvite_False()
    {
        await using var contentDbContext = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
        var repository = new UserReleaseInviteRepository(contentDbContext);
        var result = await repository.UserHasInvite(Guid.Empty, "test@test.com", ReleaseRole.Contributor);
        Assert.False(result);
    }

    [Fact]
    public async Task UserHasInvites_True()
    {
        var invite1 = new UserReleaseInvite
        {
            Email = "test@test.com",
            ReleaseVersionId = Guid.NewGuid(),
            Role = ReleaseRole.Contributor,
        };

        var invite2 = new UserReleaseInvite
        {
            Email = "test@test.com",
            ReleaseVersionId = Guid.NewGuid(),
            Role = ReleaseRole.Contributor,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.AddRangeAsync(invite1, invite2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = new UserReleaseInviteRepository(contentDbContext);
            var result = await repository.UserHasInvites(
                ListOf(invite1.ReleaseVersionId, invite2.ReleaseVersionId),
                "test@test.com", ReleaseRole.Contributor);
            Assert.True(result);
        }
    }

    [Fact]
    public async Task UserHasInvites_False()
    {
        var invite1 = new UserReleaseInvite
        {
            Email = "test@test.com",
            ReleaseVersionId = Guid.NewGuid(),
            Role = ReleaseRole.Contributor,
        };

        var invite2 = new UserReleaseInvite
        {
            Email = "test@test.com",
            ReleaseVersionId = Guid.NewGuid(),
            Role = ReleaseRole.Contributor,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.AddRangeAsync(invite1, invite2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = new UserReleaseInviteRepository(contentDbContext);
            var result = await repository.UserHasInvites(
                ListOf(invite1.ReleaseVersionId, invite2.ReleaseVersionId, Guid.NewGuid()),
                "test@test.com", ReleaseRole.Contributor);
            Assert.False(result);
        }
    }

    [Fact]
    public async Task RemoveByPublication()
    {
        var targetPublication = _fixture.DefaultPublication()
            .Generate();

        var releaseVersion1 = _fixture.DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease()
                .WithPublication(targetPublication))
            .Generate();
        var releaseVersion2 = _fixture.DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease()
                .WithPublication(_fixture.DefaultPublication()))
            .Generate();
        var releaseVersion3 = _fixture.DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease()
                .WithPublication(targetPublication))
            .Generate();

        var invite1 = _fixture.DefaultUserReleaseInvite()
            .WithEmail("test@test.com")
            .WithReleaseVersion(releaseVersion1)
            .WithRole(ReleaseRole.Contributor)
            .Generate();
        // not attached to the target publication
        var invite2 = _fixture.DefaultUserReleaseInvite()
            .WithEmail("test@test.com")
            .WithReleaseVersion(releaseVersion2)
            .WithRole(ReleaseRole.Contributor)
            .Generate();
        var invite3 = _fixture.DefaultUserReleaseInvite()
            .WithEmail("test@test.com")
            .WithReleaseVersion(releaseVersion3)
            .WithRole(ReleaseRole.Contributor)
            .Generate();
        // not ReleaseRole.Contributor
        var invite4 = _fixture.DefaultUserReleaseInvite()
            .WithEmail("test@test.com")
            .WithReleaseVersion(releaseVersion1)
            .WithRole(ReleaseRole.Approver)
            .Generate();
        // different email address
        var invite5 = _fixture.DefaultUserReleaseInvite()
            .WithEmail("test_different@test.com")
            .WithReleaseVersion(releaseVersion1)
            .WithRole(ReleaseRole.Contributor)
            .Generate();

        await using var contentDbContext = InMemoryApplicationDbContext();

        contentDbContext.UserReleaseInvites.AddRange(invite1, invite2, invite3, invite4, invite5);
        await contentDbContext.SaveChangesAsync();

        var repository = new UserReleaseInviteRepository(contentDbContext);
        await repository.RemoveByPublication(
            publicationId: targetPublication.Id,
            email: "test@test.com",
            rolesToInclude: ReleaseRole.Contributor);

        var remainingInvites = await contentDbContext.UserReleaseInvites
            .ToListAsync();

        Assert.Equal(3, remainingInvites.Count);

        Assert.Equal(invite2.Id, remainingInvites[0].Id);
        Assert.Equal(invite2.Email, remainingInvites[0].Email);
        Assert.Equal(invite2.ReleaseVersionId, remainingInvites[0].ReleaseVersionId);
        Assert.Equal(invite2.Role, remainingInvites[0].Role);

        Assert.Equal(invite4.Id, remainingInvites[1].Id);
        Assert.Equal(invite4.Email, remainingInvites[1].Email);
        Assert.Equal(invite4.ReleaseVersionId, remainingInvites[1].ReleaseVersionId);
        Assert.Equal(invite4.Role, remainingInvites[1].Role);

        Assert.Equal(invite5.Id, remainingInvites[2].Id);
        Assert.Equal(invite5.Email, remainingInvites[2].Email);
        Assert.Equal(invite5.ReleaseVersionId, remainingInvites[2].ReleaseVersionId);
        Assert.Equal(invite5.Role, remainingInvites[2].Role);
    }
}
