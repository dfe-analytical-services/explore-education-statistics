#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserReleaseRoleServiceTests
    {
        [Fact]
        public async Task ListUserReleaseRolesByPublication()
        {
            var release1 = new Release
            {
                Id = Guid.NewGuid(),
            };
            var release2 = new Release
            {
                Id = Guid.NewGuid(),
            };
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Releases = ListOf(release1, release2),
            };

            var releaseIgnored1 = new Release // Ignored because different publication
            {
                Id = Guid.NewGuid(),
                PublicationId = Guid.NewGuid(),
            };

            var userReleaseRole1 = new UserReleaseRole
            {
                User = new User{ Id = Guid.NewGuid() },
                Release = release1,
                Role = Contributor,
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                User = new User{ Id = Guid.NewGuid() },
                Release = release1,
                Role = Contributor,
            };
            var userReleaseRole3 = new UserReleaseRole
            {
                User = new User{ Id = Guid.NewGuid() },
                Release = release2,
                Role = Contributor,
            };
            var userReleaseRoleIgnored1 = new UserReleaseRole // Ignored because not Contributor role
            {
                User = new User{ Id = Guid.NewGuid() },
                Release = release1,
                Role = Lead,
            };
            var userReleaseRoleIgnored2 = new UserReleaseRole // Ignored because Deleted set
            {
                User = new User{ Id = Guid.NewGuid() },
                Release = release1,
                Role = Contributor,
                Deleted = DateTime.UtcNow,
            };
            var userReleaseRoleIgnored3 = new UserReleaseRole // Ignored due to release under different publication
            {
                User = new User{ Id = Guid.NewGuid() },
                Release = releaseIgnored1,
                Role = Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    publication,
                    userReleaseRole1, userReleaseRole2, userReleaseRole3,
                    userReleaseRoleIgnored1, userReleaseRoleIgnored2, userReleaseRoleIgnored3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleService(contentDbContext);
                var userReleaseRoles = await service.ListUserReleaseRolesByPublication(Contributor,
                    publication.Id);

                Assert.Equal(3, userReleaseRoles.Count);

                Assert.Equal(userReleaseRole1.Id, userReleaseRoles[0].Id);
                Assert.Equal(userReleaseRole1.UserId, userReleaseRoles[0].UserId);
                Assert.Equal(userReleaseRole1.ReleaseId, userReleaseRoles[0].ReleaseId);
                Assert.Equal(userReleaseRole1.Role, userReleaseRoles[0].Role);

                Assert.Equal(userReleaseRole2.Id, userReleaseRoles[1].Id);
                Assert.Equal(userReleaseRole2.UserId, userReleaseRoles[1].UserId);
                Assert.Equal(userReleaseRole2.ReleaseId, userReleaseRoles[1].ReleaseId);
                Assert.Equal(userReleaseRole2.Role, userReleaseRoles[1].Role);

                Assert.Equal(userReleaseRole3.Id, userReleaseRoles[2].Id);
                Assert.Equal(userReleaseRole3.UserId, userReleaseRoles[2].UserId);
                Assert.Equal(userReleaseRole3.ReleaseId, userReleaseRoles[2].ReleaseId);
                Assert.Equal(userReleaseRole3.Role, userReleaseRoles[2].Role);
            }
        }

        private static UserReleaseRoleService SetupUserReleaseRoleService(
            ContentDbContext contentDbContext)
        {
            return new(
                contentDbContext,
                new PublicationRepository(contentDbContext));
        }
    }
}
