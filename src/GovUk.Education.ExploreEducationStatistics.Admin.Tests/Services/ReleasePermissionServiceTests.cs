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
                ReleaseName = "2000",
            };
            var release2 = new Release
            {
                Publication = publication,
                ReleaseName = "2001",
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
            var userReleaseRole3 = new UserReleaseRole
            {
                User = user2,
                Release = release1,
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
                    userReleaseRole1, userReleaseRole2, userReleaseRole3,
                    userReleaseRoleIgnored1, userReleaseRoleIgnored2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result = await service.GetManageAccessPageContributorList(publication.Id);
                var viewModel = result.AssertRight();

                Assert.Equal(publication.Id, viewModel.PublicationId);
                Assert.Equal(publication.Title, viewModel.PublicationTitle);
                Assert.Equal(2, viewModel.Releases.Count);

                Assert.Equal(release1.Id, viewModel.Releases[0].ReleaseId);
                Assert.Equal(release1.Title, viewModel.Releases[0].ReleaseTitle);
            }
        }

        [Fact]
        public async Task GetManageAccessPageContributorList_NoPublication()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupReleasePermissionService(contentDbContext);

            var result = await service.GetManageAccessPageContributorList(Guid.NewGuid());
            result.AssertNotFound();
        }

        [Fact]
        public async Task GetManageAccessPageContributorList_NoReleases()
        {
            var publication = new Publication { Title = "Test Publication" };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result = await service.GetManageAccessPageContributorList(publication.Id);
                var viewModel = result.AssertRight();

                Assert.Equal(publication.Id, viewModel.PublicationId);
                Assert.Equal(publication.Title, viewModel.PublicationTitle);
                Assert.Empty(viewModel.Releases);
            }
        }

        [Fact]
        public async Task GetManageAccessPageContributorList_NoUserReleaseRoles()
        {
            var publication = new Publication();
            var release1 = new Release
            {
                ReleaseName = "2000",
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
                var service = SetupReleasePermissionService(contentDbContext);

                var result = await service.GetManageAccessPageContributorList(publication.Id);
                var viewModel = result.AssertRight();

                Assert.Equal(publication.Id, viewModel.PublicationId);
                Assert.Equal(publication.Title, viewModel.PublicationTitle);
                Assert.Single(viewModel.Releases);
                Assert.Empty(viewModel.Releases[0].UserList);
            }
        }

        private static ReleasePermissionService SetupReleasePermissionService(
            ContentDbContext contentDbContext,
            IUserService? userService = null)
        {
            return new(
                contentDbContext,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                new PublicationRepository(contentDbContext, AdminMapper()),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}
