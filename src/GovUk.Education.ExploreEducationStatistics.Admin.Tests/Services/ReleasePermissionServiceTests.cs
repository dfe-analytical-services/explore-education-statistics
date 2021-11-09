#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleasePermissionServiceTests
    {
        [Fact]
        public async Task GetManageAccessPageContributorList()
        {
            var publication = new Publication();
            var release1 = new Release
            {
                Publication = publication,
            };
            var release2 = new Release
            {
                Publication = publication,
            };
            var user1 = new User
            {
                FirstName = "User1",
                LastName = "One",
            };
            var userReleaseRole1 = new UserReleaseRole
            {
                User = user1,
                Release = release1,
                Role = Contributor,
            };

            var user2 = new User
            {
                FirstName = "User2",
                LastName = "Two",
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                User = user2,
                Release = release2,
                Role = Contributor,
            };

            var user3 = new User();
            var userReleaseRoleIgnored1 = new UserReleaseRole
            {
                User = user3,
                Release = new Release { Publication = new Publication() },
                Role = Contributor,
            };
            var userReleaseRoleIgnored2 = new UserReleaseRole
            {
                User = user3,
                Release = release1,
                Role = PrereleaseViewer,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, release2,
                    userReleaseRole1, userReleaseRole2,
                    userReleaseRoleIgnored1, userReleaseRoleIgnored2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext: contentDbContext);

                var result = await service.GetManageAccessPageContributorList(release1.Id);
                var manageAccessList = result.AssertRight();

                Assert.Equal(2, manageAccessList.Count);

                Assert.Equal(release1.Id, manageAccessList[0].ReleaseId);
                Assert.Equal(userReleaseRole1.Id, manageAccessList[0].ReleaseRoleId);
                Assert.Equal("User1 One", manageAccessList[0].UserFullName);
                Assert.Equal(user1.Id, manageAccessList[0].UserId);

                // Appears because has contributor role on another release under the same publication as release1
                Assert.Equal(release1.Id, manageAccessList[1].ReleaseId);
                Assert.Null(manageAccessList[1].ReleaseRoleId);
                Assert.Equal("User2 Two", manageAccessList[1].UserFullName);
                Assert.Equal(user2.Id, manageAccessList[1].UserId);
            }
        }

        [Fact]
        public async Task GetManageAccessPageContributorList_NoRelease()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupReleasePermissionService(contentDbContext: contentDbContext);

            var result = await service.GetManageAccessPageContributorList(Guid.NewGuid());
            result.AssertNotFound();
        }

        [Fact]
        public async Task GetManageAccessPageContributorList_NoUserReleaseRoles()
        {
            var publication = new Publication();
            var release1 = new Release
            {
                Publication = publication,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext: contentDbContext);

                var result = await service.GetManageAccessPageContributorList(release1.Id);
                var manageAccessList = result.AssertRight();

                Assert.Empty(manageAccessList);
            }
        }

        private ReleasePermissionService SetupReleasePermissionService(
            ContentDbContext contentDbContext,
            IUserService? userService = null)
        {
            return new ReleasePermissionService(
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                contentDbContext,
                new PublicationRepository(contentDbContext, AdminMapper()),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}
