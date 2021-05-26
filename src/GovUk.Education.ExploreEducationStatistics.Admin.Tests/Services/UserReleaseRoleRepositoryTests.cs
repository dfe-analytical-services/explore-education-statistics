using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task Create()
        {
            var user = new User();

            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.Create(user.Id, release.Id, Contributor);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(release.Id, result.ReleaseId);
                Assert.Equal(Contributor, result.Role);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();
                Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, userReleaseRoles[0].Id);
                Assert.Equal(user.Id, userReleaseRoles[0].UserId);
                Assert.Equal(release.Id, userReleaseRoles[0].ReleaseId);
                Assert.Equal(Contributor, userReleaseRoles[0].Role);
            }
        }

        [Fact]
        public async Task GetByUserAndRole()
        {
            var userReleaseRole = new UserReleaseRole
            {
                User = new User(),
                Release = new Release(),
                Role = Contributor
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.GetByUserAndRole(userReleaseRole.UserId, userReleaseRole.ReleaseId, Contributor);

                Assert.Equal(userReleaseRole.Id, result.Id);
                Assert.Equal(userReleaseRole.UserId, result.UserId);
                Assert.Equal(userReleaseRole.ReleaseId, result.ReleaseId);
                Assert.Equal(Contributor, result.Role);
            }
        }

        private static UserReleaseRoleRepository SetupUserReleaseRoleRepository(
            ContentDbContext contentDbContext)
        {
            return new UserReleaseRoleRepository(
                contentDbContext
            );
        }
    }
}
