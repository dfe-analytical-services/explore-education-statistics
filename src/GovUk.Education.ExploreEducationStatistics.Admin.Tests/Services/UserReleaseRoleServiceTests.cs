#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserReleaseRoleServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task ListUserReleaseRolesByPublication()
    {
        var (publication, publicationIgnored) = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .Generate(2))
            .GenerateTuple2();

        var userReleaseRole1 = new UserReleaseRole
        {
            User = new User { Id = Guid.NewGuid() },
            ReleaseVersion = publication.ReleaseVersions[0],
            Role = Contributor,
        };
        var userReleaseRole2 = new UserReleaseRole
        {
            User = new User { Id = Guid.NewGuid() },
            ReleaseVersion = publication.ReleaseVersions[0],
            Role = Contributor,
        };
        var userReleaseRole3 = new UserReleaseRole
        {
            User = new User { Id = Guid.NewGuid() },
            ReleaseVersion = publication.ReleaseVersions[1],
            Role = Contributor,
        };
        var userReleaseRoleIgnored1 = new UserReleaseRole // Ignored because not Contributor role
        {
            User = new User { Id = Guid.NewGuid() },
            ReleaseVersion = publication.ReleaseVersions[0],
            Role = Approver,
        };
        var userReleaseRoleIgnored2 = new UserReleaseRole // Ignored due to release under different publication
        {
            User = new User { Id = Guid.NewGuid() },
            ReleaseVersion = publicationIgnored.ReleaseVersions[0],
            Role = Contributor,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.AddRange(publication, publicationIgnored);
            contentDbContext.UserReleaseRoles.AddRange(
                userReleaseRole1, userReleaseRole2, userReleaseRole3,
                userReleaseRoleIgnored1, userReleaseRoleIgnored2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildService(contentDbContext);
            var userReleaseRoles = await service.ListUserReleaseRolesByPublication(Contributor,
                publication.Id);

            Assert.Equal(3, userReleaseRoles.Count);

            Assert.Equal(userReleaseRole1.Id, userReleaseRoles[0].Id);
            Assert.Equal(userReleaseRole1.UserId, userReleaseRoles[0].UserId);
            Assert.Equal(userReleaseRole1.ReleaseVersionId, userReleaseRoles[0].ReleaseVersionId);
            Assert.Equal(userReleaseRole1.Role, userReleaseRoles[0].Role);

            Assert.Equal(userReleaseRole2.Id, userReleaseRoles[1].Id);
            Assert.Equal(userReleaseRole2.UserId, userReleaseRoles[1].UserId);
            Assert.Equal(userReleaseRole2.ReleaseVersionId, userReleaseRoles[1].ReleaseVersionId);
            Assert.Equal(userReleaseRole2.Role, userReleaseRoles[1].Role);

            Assert.Equal(userReleaseRole3.Id, userReleaseRoles[2].Id);
            Assert.Equal(userReleaseRole3.UserId, userReleaseRoles[2].UserId);
            Assert.Equal(userReleaseRole3.ReleaseVersionId, userReleaseRoles[2].ReleaseVersionId);
            Assert.Equal(userReleaseRole3.Role, userReleaseRoles[2].Role);
        }
    }

    private static UserReleaseRoleService BuildService(
        ContentDbContext contentDbContext)
    {
        return new(
            contentDbContext,
            new ReleaseVersionRepository(contentDbContext));
    }
}
