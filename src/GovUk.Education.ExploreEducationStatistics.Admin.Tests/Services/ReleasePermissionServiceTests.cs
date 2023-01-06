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
        private static readonly Guid UserId = Guid.NewGuid();

        [Fact]
        public async Task ListReleaseRoles()
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
            var user3ReleaseRoleIgnored1 = new UserReleaseRole // Ignored because different publication
            {
                User = user3,
                Release = new Release { Publication = new Publication() },
                Role = Contributor,
            };
            var user3ReleaseRole = new UserReleaseRole
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
                    user3ReleaseRoleIgnored1, user3ReleaseRole, user3ReleaseRoleIgnored3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result = await service.ListReleaseRoles(release1.Id);
                var viewModel = result.AssertRight();

                Assert.Equal(3, viewModel.Count);

                Assert.Equal(user1.Id, viewModel[0].UserId);
                Assert.Equal(user1.DisplayName, viewModel[0].UserDisplayName);
                Assert.Equal(user1.Email, viewModel[0].UserEmail);
                Assert.Equal(Contributor, viewModel[0].Role);

                Assert.Equal(user2.Id, viewModel[1].UserId);
                Assert.Equal(user2.DisplayName, viewModel[1].UserDisplayName);
                Assert.Equal(user2.Email, viewModel[1].UserEmail);
                Assert.Equal(Contributor, viewModel[1].Role);

                Assert.Equal(user3.Id, viewModel[2].UserId);
                Assert.Equal(user3.DisplayName, viewModel[2].UserDisplayName);
                Assert.Equal(user3.Email, viewModel[2].UserEmail);
                Assert.Equal(PrereleaseViewer, viewModel[2].Role);
            }
        }
        
        [Fact]
        public async Task ListReleaseRoles_ContributorsOnly()
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

                var result = await service.ListReleaseRoles(release1.Id, new [] { Contributor });
                var viewModel = result.AssertRight();

                Assert.Equal(2, viewModel.Count);

                Assert.Equal(user1.Id, viewModel[0].UserId);
                Assert.Equal(user1.DisplayName, viewModel[0].UserDisplayName);
                Assert.Equal(user1.Email, viewModel[0].UserEmail);
                Assert.Equal(Contributor, viewModel[0].Role);

                Assert.Equal(user2.Id, viewModel[1].UserId);
                Assert.Equal(user2.DisplayName, viewModel[1].UserDisplayName);
                Assert.Equal(user2.Email, viewModel[1].UserEmail);
                Assert.Equal(Contributor, viewModel[1].Role);
            }
        }

        [Fact]
        public async Task ListReleaseRoles_NoPublication()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupReleasePermissionService(contentDbContext);

            var result = await service.ListReleaseRoles(Guid.NewGuid());
            result.AssertNotFound();
        }

        [Fact]
        public async Task ListReleaseRoles_NoRelease()
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

                var result = await service.ListReleaseRoles(Guid.NewGuid());
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ListReleaseRoles_NoUserReleaseRoles()
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
                    .ListReleaseRoles(release1.Id);
                var viewModel = result.AssertRight();

                Assert.Empty(viewModel);
            }
        }

        [Fact]
        public async Task ListReleaseInvites()
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

            var user1ReleaseInvite = new UserReleaseInvite
            {
                Email = "user1@test.com",
                Release = release1,
                Role = Contributor,
            };

            var user2ReleaseInviteIgnored = new UserReleaseInvite
            {
                Email = "user2@test.com",
                Release = release2Amendment, // ignored because not release1
                Role = Contributor,
            };

            var user3ReleaseInvite = new UserReleaseInvite
            {
                Email = "user3@test.com",
                Release = release1,
                Role = Contributor,
            };

            var user4ReleaseInviteIgnored = new UserReleaseInvite
            {
                Email = "user4@test.com",
                Release = release1,
                Role = Lead
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, release2Original, release2Amendment,
                    user1ReleaseInvite, user2ReleaseInviteIgnored, user3ReleaseInvite,
                    user4ReleaseInviteIgnored);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result = await service.ListReleaseInvites(release1.Id);
                var viewModel = result.AssertRight();

                Assert.Equal(3, viewModel.Count);
                
                Assert.Equal("user1@test.com", viewModel[0].Email);
                Assert.Equal(Contributor, viewModel[0].Role);
                
                Assert.Equal("user3@test.com", viewModel[1].Email);
                Assert.Equal(Contributor, viewModel[1].Role);
                
                Assert.Equal("user4@test.com", viewModel[2].Email);
                Assert.Equal(Lead, viewModel[2].Role);
            }
        }

        [Fact]
        public async Task ListReleaseInvites_ContributorsOnly()
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

            var user1ReleaseInvite = new UserReleaseInvite
            {
                Email = "user1@test.com",
                Release = release1,
                Role = Contributor,
            };

            var user2ReleaseInviteIgnored = new UserReleaseInvite
            {
                Email = "user2@test.com",
                Release = release2Amendment, // ignored because not release1
                Role = Contributor,
            };

            var user3ReleaseInvite = new UserReleaseInvite
            {
                Email = "user3@test.com",
                Release = release1,
                Role = Contributor,
            };

            var user3ReleaseInviteIgnored = new UserReleaseInvite
            {
                Email = "user4@test.com",
                Release = release1,
                Role = Lead, // ignored because not a Contributor
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, release2Original, release2Amendment,
                    user1ReleaseInvite, user2ReleaseInviteIgnored, user3ReleaseInvite,
                    user3ReleaseInviteIgnored);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleasePermissionService(contentDbContext);

                var result = await service.ListReleaseInvites(release1.Id, new [] { Contributor });
                var viewModel = result.AssertRight();

                Assert.Equal(2, viewModel.Count);
                
                Assert.Equal("user1@test.com", viewModel[0].Email);
                Assert.Equal(Contributor, viewModel[0].Role);
                
                Assert.Equal("user3@test.com", viewModel[1].Email);
                Assert.Equal(Contributor, viewModel[1].Role);
            }
        }

        [Fact]
        public async Task ListReleaseInvites_NoPublication()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupReleasePermissionService(contentDbContext);

            var result = await service.ListReleaseInvites(Guid.NewGuid());
            result.AssertNotFound();
        }

        [Fact]
        public async Task ListReleaseInvites_NoRelease()
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

                var result = await service.ListReleaseInvites(Guid.NewGuid());
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ListReleaseInvites_NoUserReleaseInvites()
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
                    .ListReleaseInvites(release1.Id);
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
                Assert.Equal(UserId, userReleaseRoles[1].CreatedById);
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

            var user1Release1Invite = new UserReleaseInvite
            {
                Email = user1.Email,
                Release = release1,
                Role = Contributor,
            };
            var user2Release2Invite = new UserReleaseInvite
            {
                Email = user2.Email,
                Release = release2Original,
                Role = Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, release2Original, release2Amendment,
                    user1ReleaseRole1, user2ReleaseRole1, user2ReleaseRole2, user3ReleaseRole1,
                    user1Release1Invite, user2Release2Invite);
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

                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .ToAsyncEnumerable()
                    .ToListAsync();

                Assert.Single(userReleaseInvites); // user1's invite remains
                Assert.Equal(user1Release1Invite.Id, userReleaseInvites[0].Id);
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
                userService ?? MockUtils.AlwaysTrueUserService(UserId).Object
            );
        }
    }
}
