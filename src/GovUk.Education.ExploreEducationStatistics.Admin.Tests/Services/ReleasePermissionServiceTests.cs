#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleasePermissionServiceTests
    {
        private static readonly Guid _userId = Guid.NewGuid();

        [Fact]
        public async Task ListReleaseContributors()
        {
            var publication = new Publication();
            var release1 = new Release
            {
                Publication = publication,
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var release2Original = new Release
            {
                Publication = publication,
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var release2Amendment = new Release
            {
                Publication = publication,
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                PreviousVersion = release2Original,
            };
            var user1 = new User
            {
                FirstName = "User1",
                LastName = "One",
                Email = "user1@test.com",
            };
            var user1ReleaseRole1 = new UserReleaseRole
            {
                User = user1,
                Release = release1,
                Role = Contributor,
            };

            var user2 = new User
            {
                FirstName = "User2",
                LastName = "Two",
                Email = "user2@test.com",
            };
            var user2ReleaseRole1 = new UserReleaseRole
            {
                User = user2,
                Release = release2Amendment,
                Role = Contributor,
            };
            var user2ReleaseRole2 = new UserReleaseRole
            {
                User = user2,
                Release = release1,
                Role = Contributor,
            };

            var user3 = new User();
            var user3ReleaseRoleIgnored1 = new UserReleaseRole // Ignored because different publication
            {
                User = user3,
                Release = new Release { Publication = new Publication() },
                Role = Contributor,
            };
            var user3ReleaseRoleIgnored2 = new UserReleaseRole // Ignored because not Contributor role
            {
                User = user3,
                Release = release1,
                Role = PrereleaseViewer,
            };
            var user3ReleaseRoleIgnored3 = new UserReleaseRole // Ignored because different release
            {
                User = user3,
                Release = release2Original,
                Role = Contributor,
                Deleted = DateTime.UtcNow,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, release2Original, release2Amendment,
                    user1ReleaseRole1, user2ReleaseRole1, user2ReleaseRole2,
                    user3ReleaseRoleIgnored1, user3ReleaseRoleIgnored2, user3ReleaseRoleIgnored3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result = await service.ListReleaseContributors(release1.Id);
                var viewModel = result.AssertRight();

                Assert.Equal(2, viewModel.Count);

                Assert.Equal(user1.Id, viewModel[0].UserId);
                Assert.Equal(user1.DisplayName, viewModel[0].UserDisplayName);
                Assert.Equal(user1.Email, viewModel[0].UserEmail);

                Assert.Equal(user2.Id, viewModel[1].UserId);
                Assert.Equal(user2.DisplayName, viewModel[1].UserDisplayName);
                Assert.Equal(user2.Email, viewModel[1].UserEmail);
            }
        }

        [Fact]
        public async Task ListReleaseContributors_NoPublication()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupReleasePermissionService(contentDbContext);

            var result = await service.ListReleaseContributors(Guid.NewGuid());
            result.AssertNotFound();
        }

        [Fact]
        public async Task ListReleaseContributors_NoRelease()
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

                var result = await service.ListReleaseContributors(Guid.NewGuid());
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ListReleaseContributors_NoUserReleaseRoles()
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

                var result = await service
                    .ListReleaseContributors(release1.Id);
                var viewModel = result.AssertRight();

                Assert.Empty(viewModel);
            }
        }

        [Fact]
        public async Task ListPublicationContributors()
        {
            var publication = new Publication();
            var release1 = new Release
            {
                Publication = publication,
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var release2Original = new Release
            {
                Publication = publication,
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var release2Amendment = new Release
            {
                Publication = publication,
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                PreviousVersion = release2Original,
            };
            var user1 = new User
            {
                FirstName = "User1",
                LastName = "One",
                Email = "user1@test.com",
            };
            var user1ReleaseRole1 = new UserReleaseRole
            {
                User = user1,
                Release = release1,
                Role = Contributor,
            };

            var user2 = new User
            {
                FirstName = "User2",
                LastName = "Two",
                Email = "user2@test.com",
            };
            var user2ReleaseRole1 = new UserReleaseRole
            {
                User = user2,
                Release = release2Amendment,
                Role = Contributor,
            };

            var user3 = new User();
            var user3ReleaseRoleIgnored1 = new UserReleaseRole // Ignored because different publication
            {
                User = user3,
                Release = new Release { Publication = new Publication() },
                Role = Contributor,
            };
            var user3ReleaseRoleIgnored2 = new UserReleaseRole // Ignored because not Contributor role
            {
                User = user3,
                Release = release1,
                Role = PrereleaseViewer,
            };
            var user3ReleaseRoleIgnored3 = new UserReleaseRole // Ignored because not latest version of release
            {
                User = user3,
                Release = release2Original,
                Role = Contributor,
                Deleted = DateTime.UtcNow,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, release2Original, release2Amendment,
                    user1ReleaseRole1, user2ReleaseRole1,
                    user3ReleaseRoleIgnored1, user3ReleaseRoleIgnored2, user3ReleaseRoleIgnored3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result =
                    await service.ListPublicationContributors(publication.Id);
                var viewModel = result.AssertRight();

                Assert.Equal(2, viewModel.Count);

                Assert.Equal(user1.Id, viewModel[0].UserId);
                Assert.Equal(user1.DisplayName, viewModel[0].UserDisplayName);
                Assert.Equal(user1.Email, viewModel[0].UserEmail);

                Assert.Equal(user2.Id, viewModel[1].UserId);
                Assert.Equal(user2.DisplayName, viewModel[1].UserDisplayName);
                Assert.Equal(user2.Email, viewModel[1].UserEmail);
            }
        }

        [Fact]
        public async Task ListPublicationContributors_NoPublication()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var service = SetupReleasePermissionService(contentDbContext);

            var result =
                await service.ListPublicationContributors(Guid.NewGuid());
            result.AssertNotFound();
        }


        [Fact]
        public async Task ListPublicationContributors_NoRelease()
        {
            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result =
                    await service.ListPublicationContributors(publication.Id);
                var viewModel = result.AssertRight();

                Assert.Empty(viewModel);
            }
        }

        [Fact]
        public async Task ListPublicationContributors_NoUserReleaseRoles()
        {
            var publication = new Publication();
            var release1 = new Release
            {
                Publication = publication,
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
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

                var result =
                    await service.ListPublicationContributors(publication.Id);
                var viewModel = result.AssertRight();

                Assert.Empty(viewModel);
            }
        }

        [Fact]
        public async Task UpdateReleaseContributors()
        {
            var publication = new Publication();
            var release1 = new Release
            {
                Publication = publication,
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var user1 = new User
            {
                FirstName = "User1",
                LastName = "One",
                Email = "user1@test.com",
            };
            var user1ReleaseRole1 = new UserReleaseRole
            {
                User = user1,
                Release = release1,
                Role = Contributor,
                Created = new DateTime(2000, 12, 25),
                CreatedById = Guid.NewGuid(),
            };
            var user2 = new User
            {
                FirstName = "User2",
                LastName = "Two",
                Email = "user2@test.com",
            };
            var user2ReleaseRole1 = new UserReleaseRole
            {
                User = user2,
                Release = release1,
                Role = Contributor,
            };
            var user3 = new User();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, user1ReleaseRole1, user2ReleaseRole1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result =
                    await service.UpdateReleaseContributors(release1.Id, ListOf(user1.Id, user3.Id));
                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsAsyncEnumerable()
                    .Where(urr =>
                        urr.ReleaseId == release1.Id
                        && urr.Role == Contributor)
                    .ToListAsync();

                Assert.Equal(2, userReleaseRoles.Count);

                Assert.Equal(user1ReleaseRole1.Id, userReleaseRoles[0].Id);
                Assert.Equal(user1.Id, userReleaseRoles[0].UserId);
                Assert.Equal(user1ReleaseRole1.Created, userReleaseRoles[0].Created);
                Assert.Equal(user1ReleaseRole1.CreatedById, userReleaseRoles[0].CreatedById);

                // userReleaseRole[1] is newly created, so cannot check Id
                Assert.Equal(user3.Id, userReleaseRoles[1].UserId);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[1].Created!.Value).Milliseconds,
                    0, 1500);
                Assert.Equal(_userId, userReleaseRoles[1].CreatedById);
            }
        }

        [Fact]
        public async Task UpdateReleaseContributors_RemoveAllContributorsFromRelease()
        {
            var publication = new Publication();
            var release1 = new Release
            {
                Publication = publication,
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };

            var user1 = new User
            {
                FirstName = "User1",
                LastName = "One",
                Email = "user1@test.com",
            };
            var user1ReleaseRole1 = new UserReleaseRole
            {
                User = user1,
                Release = release1,
                Role = Contributor,
            };

            var user2 = new User
            {
                FirstName = "User2",
                LastName = "Two",
                Email = "user2@test.com",
            };
            var user2ReleaseRole1 = new UserReleaseRole
            {
                User = user2,
                Release = release1,
                Role = Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, user1ReleaseRole1, user2ReleaseRole1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result =
                    await service.UpdateReleaseContributors(release1.Id, new List<Guid>());
                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsAsyncEnumerable()
                    .Where(urr =>
                        urr.ReleaseId == release1.Id
                        && urr.Role == Contributor)
                    .ToListAsync();

                Assert.Empty(userReleaseRoles);
            }
        }

        [Fact]
        public async Task RemoveAllUserContributorPermissionForPublication()
        {
            var publication = new Publication();
            var release1 = new Release
            {
                Publication = publication,
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var release2Original = new Release
            {
                Publication = publication,
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var release2Amendment = new Release
            {
                Publication = publication,
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                PreviousVersion = release2Original,
            };
            var user1 = new User
            {
                FirstName = "User1",
                LastName = "One",
                Email = "user1@test.com",
            };
            var user1ReleaseRole1 = new UserReleaseRole
            {
                User = user1,
                Release = release1,
                Role = Contributor,
            };

            var user2 = new User
            {
                FirstName = "User2",
                LastName = "Two",
                Email = "user2@test.com",
            };
            var user2ReleaseRole1 = new UserReleaseRole
            {
                User = user2,
                Release = release2Amendment,
                Role = Contributor,
            };
            var user2ReleaseRole2 = new UserReleaseRole
            {
                User = user2,
                Release = release1,
                Role = Contributor,
            };

            var user3 = new User
            {
                FirstName = "User3",
                LastName = "Three",
                Email = "user3@test.com",
            };
            var user3ReleaseRole1 = new UserReleaseRole
            {
                User = user3,
                Release = release2Amendment,
                Role = Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, release2Original, release2Amendment,
                    user1ReleaseRole1, user2ReleaseRole1, user2ReleaseRole2, user3ReleaseRole1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result =
                    await service.RemoveAllUserContributorPermissionsForPublication(publication.Id, user2.Id);
                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsAsyncEnumerable()
                    .Where(urr => urr.Role == Contributor)
                    .ToListAsync();

                Assert.Equal(2, userReleaseRoles.Count);
                Assert.Equal(user1.Id, userReleaseRoles[0].UserId);
                Assert.Equal(user3.Id, userReleaseRoles[1].UserId);
            }
        }

        private static ReleasePermissionService SetupReleasePermissionService(
            ContentDbContext contentDbContext,
            IUserService? userService = null)
        {
            return new(
                contentDbContext,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                new UserReleaseRoleRepository(contentDbContext),
                new UserReleaseInviteRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService(_userId).Object
            );
        }
    }
}
