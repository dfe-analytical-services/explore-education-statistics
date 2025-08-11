#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserReleaseRoleAndInviteManagerTests
{
    private readonly DataFixture _fixture = new();

    public class CreateTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task Create()
        {
            var user = new User();

            var createdByUser = new User();

            var releaseVersion = new ReleaseVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.Users.AddRange(user, createdByUser);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                var result = await service.Create(userId: user.Id,
                    resourceId: releaseVersion.Id,
                    ReleaseRole.Contributor,
                    createdById: createdByUser.Id);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, result.Role);
                Assert.Equal(createdByUser.Id, result.CreatedById);
                result.Created.AssertUtcNow();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRole = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .SingleAsync();

                Assert.NotEqual(Guid.Empty, userReleaseRole.Id);
                Assert.Equal(user.Id, userReleaseRole.UserId);
                Assert.Equal(releaseVersion.Id, userReleaseRole.ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRole.Role);
                Assert.Equal(createdByUser.Id, userReleaseRole.CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRole.Created!.Value).Milliseconds,
                    0, 1500);
            }
        }
    }

    public class CreateIfNotExistsTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task CreateIfNotExists_DoesNotExist()
        {
            var userId = Guid.NewGuid();
            var createdById = Guid.NewGuid();
            var releaseVersionId = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                var result = await service.CreateIfNotExists(userId: userId,
                    resourceId: releaseVersionId,
                    ReleaseRole.Contributor,
                    createdById: createdById);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(userId, result.UserId);
                Assert.Equal(releaseVersionId, result.ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, result.Role);
                Assert.Equal(createdById, result.CreatedById);
                result.Created.AssertUtcNow();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRole = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .SingleAsync();

                Assert.NotEqual(Guid.Empty, userReleaseRole.Id);
                Assert.Equal(userId, userReleaseRole.UserId);
                Assert.Equal(releaseVersionId, userReleaseRole.ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRole.Role);
                Assert.Equal(createdById, userReleaseRole.CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRole.Created!.Value).Milliseconds,
                    0, 1500);
            }
        }

        [Fact]
        public async Task CreateIfNotExists_AlreadyExists()
        {
            var userReleaseRole = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Approver,
                CreatedById = Guid.NewGuid(),
                Created = new DateTime(2021, 12, 25),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                var result = await service.CreateIfNotExists(userId: userReleaseRole.UserId,
                    resourceId: userReleaseRole.ReleaseVersionId,
                    userReleaseRole.Role,
                    createdById: userReleaseRole.CreatedById.Value);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(userReleaseRole.UserId, result.UserId);
                Assert.Equal(userReleaseRole.ReleaseVersionId, result.ReleaseVersionId);
                Assert.Equal(userReleaseRole.Role, result.Role);
                Assert.Equal(userReleaseRole.CreatedById, result.CreatedById);
                Assert.Equal(userReleaseRole.Created, result.Created);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var dbUserReleaseRole = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .SingleAsync();

                Assert.NotEqual(Guid.Empty, dbUserReleaseRole.Id);
                Assert.Equal(dbUserReleaseRole.UserId, dbUserReleaseRole.UserId);
                Assert.Equal(dbUserReleaseRole.ReleaseVersionId, dbUserReleaseRole.ReleaseVersionId);
                Assert.Equal(dbUserReleaseRole.Role, dbUserReleaseRole.Role);
                Assert.Equal(dbUserReleaseRole.CreatedById, dbUserReleaseRole.CreatedById);
                Assert.Equal(dbUserReleaseRole.Created, dbUserReleaseRole.Created);
            }
        }
    }

    public class CreateManyIfNotExistsTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task CreateManyIfNotExists_Users()
        {
            var releaseVersion = new ReleaseVersion();

            var user1 = new User();
            var user1ReleaseRole1 = new UserReleaseRole
            {
                ReleaseVersion = releaseVersion,
                User = user1,
                Role = ReleaseRole.Contributor,
                CreatedById = Guid.NewGuid(),
                Created = new DateTime(2000, 12, 25),
            };

            var user2 = new User();
            var user3 = new User();
            var user4 = new User();

            var createdByUser = new User();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    user1ReleaseRole1, user2, user3, user4, createdByUser);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                await service.CreateManyIfNotExists(
                    userIds: ListOf(user1.Id, user2.Id, user3.Id, user4.Id),
                    resourceId: releaseVersion.Id,
                    role: ReleaseRole.Contributor,
                    createdById: createdByUser.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(4, userReleaseRoles.Count);

                Assert.Equal(user1.Id, userReleaseRoles[0].UserId);
                Assert.Equal(releaseVersion.Id, userReleaseRoles[0].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[0].Role);
                Assert.Equal(user1ReleaseRole1.CreatedById, userReleaseRoles[0].CreatedById);
                Assert.Equal(user1ReleaseRole1.Created, userReleaseRoles[0].Created);

                Assert.Equal(user2.Id, userReleaseRoles[1].UserId);
                Assert.Equal(releaseVersion.Id, userReleaseRoles[1].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[1].Role);
                Assert.Equal(createdByUser.Id, userReleaseRoles[1].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[1].Created!.Value).Milliseconds,
                    0, 1500);

                Assert.Equal(user3.Id, userReleaseRoles[2].UserId);
                Assert.Equal(releaseVersion.Id, userReleaseRoles[2].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[2].Role);
                Assert.Equal(createdByUser.Id, userReleaseRoles[2].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[2].Created!.Value).Milliseconds,
                    0, 1500);

                Assert.Equal(user4.Id, userReleaseRoles[3].UserId);
                Assert.Equal(releaseVersion.Id, userReleaseRoles[3].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[3].Role);
                Assert.Equal(createdByUser.Id, userReleaseRoles[3].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[3].Created!.Value).Milliseconds,
                    0, 1500);
            }
        }

        [Fact]
        public async Task CreateManyIfNotExists_Releases()
        {
            var release1 = new ReleaseVersion();

            var user = new User();
            var createdByUser = new User();

            var userRelease1Role = new UserReleaseRole
            {
                ReleaseVersion = release1,
                User = user,
                Role = ReleaseRole.Contributor,
                CreatedBy = createdByUser,
                Created = new DateTime(2000, 12, 25),
            };
            var release2 = new ReleaseVersion();
            var release3 = new ReleaseVersion();
            var release4 = new ReleaseVersion();


            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(release1, release2, release3, release4);
                contentDbContext.UserReleaseRoles.Add(userRelease1Role);
                contentDbContext.Users.Add(createdByUser);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                await service.CreateManyIfNotExists(
                    userId: user.Id,
                    resourceIds: ListOf(release1.Id, release2.Id, release3.Id, release4.Id),
                    role: ReleaseRole.Contributor,
                    createdById: createdByUser.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(4, userReleaseRoles.Count);

                Assert.Equal(userRelease1Role.UserId, userReleaseRoles[0].UserId);
                Assert.Equal(userRelease1Role.ReleaseVersionId, userReleaseRoles[0].ReleaseVersionId);
                Assert.Equal(userRelease1Role.Role, userReleaseRoles[0].Role);
                Assert.Equal(userRelease1Role.CreatedById, userReleaseRoles[0].CreatedById);
                Assert.Equal(userRelease1Role.Created, userReleaseRoles[0].Created);

                Assert.Equal(user.Id, userReleaseRoles[1].UserId);
                Assert.Equal(release2.Id, userReleaseRoles[1].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[1].Role);
                Assert.Equal(createdByUser.Id, userReleaseRoles[1].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[1].Created!.Value).Milliseconds,
                    0, 1500);

                Assert.Equal(user.Id, userReleaseRoles[2].UserId);
                Assert.Equal(release3.Id, userReleaseRoles[2].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[2].Role);
                Assert.Equal(createdByUser.Id, userReleaseRoles[2].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[2].Created!.Value).Milliseconds,
                    0, 1500);

                Assert.Equal(user.Id, userReleaseRoles[3].UserId);
                Assert.Equal(release4.Id, userReleaseRoles[3].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[3].Role);
                Assert.Equal(createdByUser.Id, userReleaseRoles[3].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[3].Created!.Value).Milliseconds,
                    0, 1500);
            }
        }
    }

    public class CreateManyTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task CreateMany_NoUsersToAdd()
        {
            var releaseVersion = new ReleaseVersion();
            var createdByUser = new User();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.Users.Add(createdByUser);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                await service.CreateManyIfNotExists(new List<Guid>(),
                    releaseVersion.Id, ReleaseRole.Contributor, createdByUser.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseRoles);
            }
        }
    }

    public class GetAllRolesByUserAndReleaseTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task GetAllRolesByUserAndRelease()
        {
            var user = new User();
            var releaseVersion = new ReleaseVersion();

            var userReleaseRoles = new List<UserReleaseRole>
            {
                new()
                {
                    User = user,
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.Contributor
                },
                new()
                {
                    User = user,
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                }
            };

            var otherUserReleaseRoles = new List<UserReleaseRole>
            {
                // Role for different release
                new()
                {
                    User = user,
                    ReleaseVersion = new ReleaseVersion(),
                    Role = ReleaseRole.Approver
                },
                // Role for different user
                new()
                {
                    User = new User(),
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.Approver
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                contentDbContext.UserReleaseRoles.AddRange(otherUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                var result = await service.GetAllRolesByUserAndRelease(userId: user.Id,
                    releaseVersionId: releaseVersion.Id);

                Assert.Equal(2, result.Count);
                Assert.Equal(ReleaseRole.Contributor, result[0]);
                Assert.Equal(ReleaseRole.PrereleaseViewer, result[1]);
            }
        }
    }

    public class GetAllRolesByUserAndPublicationTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task GetAllRolesByUserAndPublication()
        {
            var user = new User();
            var publication = new Publication();

            var userReleaseRolesForPublication = new List<UserReleaseRole>
            {
                _fixture.DefaultUserReleaseRole()
                    .WithUser(user)
                    .WithRole(ReleaseRole.Contributor)
                    .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease()
                            .WithPublication(publication))),
                _fixture.DefaultUserReleaseRole()
                    .WithUser(user)
                    .WithRole(ReleaseRole.Approver)
                    .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease()
                            .WithPublication(publication))),
                // Add a duplicate ReleaseRole to ensure duplicates are removed by the method under test
                _fixture.DefaultUserReleaseRole()
                    .WithUser(user)
                    .WithRole(ReleaseRole.Approver)
                    .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease()
                            .WithPublication(publication)))
            };

            var otherPublication = new Publication();
            var otherUserReleaseRoles = new List<UserReleaseRole>
            {
                // Role for different Publication
                _fixture.DefaultUserReleaseRole()
                    .WithUser(user)
                    .WithRole(ReleaseRole.PrereleaseViewer)
                    .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease()
                            .WithPublication(otherPublication))),                
                // Role for same Publication but different user
               _fixture.DefaultUserReleaseRole()
                    .WithUser(new User())
                    .WithRole(ReleaseRole.PrereleaseViewer)
                    .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease()
                            .WithPublication(publication)))
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddRangeAsync(user);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRolesForPublication);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(otherUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                var result = await service.GetAllRolesByUserAndPublication(user.Id, publication.Id);

                Assert.Equal(2, result.Count);
                Assert.Equal(ReleaseRole.Contributor, result[0]);
                Assert.Equal(ReleaseRole.Approver, result[1]);
            }
        }
    }

    public class GetDistinctRolesByUserTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task GetDistinctRolesByUser()
        {
            var user = new User();
            var release1 = new ReleaseVersion();
            var release2 = new ReleaseVersion();

            var userReleaseRoles = new List<UserReleaseRole>
            {
                new()
                {
                    User = user,
                    ReleaseVersion = release1,
                    Role = ReleaseRole.Contributor
                },
                new()
                {
                    User = user,
                    ReleaseVersion = release1,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    User = user,
                    ReleaseVersion = release2,
                    Role = ReleaseRole.Approver,
                },
                new()
                {
                    User = user,
                    ReleaseVersion = release2,
                    Role = ReleaseRole.PrereleaseViewer
                }
            };

            var otherUserReleaseRoles = new List<UserReleaseRole>
            {
                // Role for different release
                new()
                {
                    User = user,
                    ReleaseVersion = new ReleaseVersion(),
                    Role = ReleaseRole.Approver
                },
                // Role for different user
                new()
                {
                    User = new User(),
                    ReleaseVersion = release1,
                    Role = ReleaseRole.Approver
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.ReleaseVersions.AddRange(release1, release2);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                contentDbContext.UserReleaseRoles.AddRange(otherUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                var result = await service.GetDistinctRolesByUser(user.Id);

                // Expect 3 distinct results.  The 4th duplicate "ReleaseRole.PrereleaseViewer" role is filtered out.
                Assert.Equal([ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], result);
            }
        }
    }

    public class GetUserReleaseRoleTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task GetUserReleaseRole_ReturnRoleIfExists()
        {
            var userReleaseRole = new UserReleaseRole
            {
                User = new User(),
                ReleaseVersion = new ReleaseVersion(),
                Role = ReleaseRole.Contributor
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                var result = await service.GetUserReleaseRole(
                    userId: userReleaseRole.UserId,
                    releaseVersionId: userReleaseRole.ReleaseVersionId,
                    ReleaseRole.Contributor);

                Assert.NotNull(result);
                Assert.Equal(userReleaseRole.UserId, result.UserId);
                Assert.Equal(userReleaseRole.ReleaseVersionId, result.ReleaseVersionId);
                Assert.Equal(userReleaseRole.Role, result.Role);
            }
        }

        [Fact]
        public async Task GetUserReleaseRole_NullIfNotExists()
        {
            var user = new User();
            var releaseVersion = new ReleaseVersion();

            // Setup a role but for a different release to make sure it has no influence
            var userReleaseRoleOtherRelease = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = new ReleaseVersion(),
                Role = ReleaseRole.Contributor
            };

            // Setup a different role on the release to make sure it has no influence
            var userReleaseRoleDifferentRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.Approver
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.UserReleaseRoles.AddRange(
                    userReleaseRoleOtherRelease,
                    userReleaseRoleDifferentRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(contentDbContext);

                var result = await service.GetUserReleaseRole(
                    userId: user.Id,
                    releaseVersionId: releaseVersion.Id,
                    ReleaseRole.Contributor);

                Assert.Null(result);
            }
        }
    }

    public class RemoveRoleAndInviteTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task Success()
        {
            var email = "test@test.com";

            var userReleaseRole = _fixture.DefaultUserReleaseRole()
                .WithUser(new User
                {
                    Email = email
                })
                .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                    .WithRelease(_fixture.DefaultRelease()
                        .WithPublication(_fixture.DefaultPublication())))
                .WithRole(ReleaseRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.Remove(
                    userReleaseRole.ReleaseVersionId,
                    email,
                    userReleaseRole.Role,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                var userReleaseRoleToRemove = await contentDbContext.UserReleaseRoles
                    .SingleAsync(urr => urr.Id == userReleaseRole.Id);

                await service.RemoveRoleAndInvite(userReleaseRoleToRemove);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedReleaseRole = await contentDbContext.UserReleaseRoles
                    .SingleOrDefaultAsync(urr => urr.Id == userReleaseRole.Id);

                Assert.Null(updatedReleaseRole);
            }
        }
    }

    public class RemoveRolesAndInvitesTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task Success()
        {
            var user1 = new User { Email = "test1@test.com" };
            var releaseVersion1 = _fixture.DefaultReleaseVersion()
                    .WithRelease(_fixture.DefaultRelease()
                        .WithPublication(_fixture.DefaultPublication()))
                    .Generate();
            var userReleaseRole1 = _fixture.DefaultUserReleaseRole()
                .WithUser(user1)
                .WithReleaseVersion(releaseVersion1)
                .WithRole(ReleaseRole.Contributor)
                .Generate();
            var userReleaseInvite1 = _fixture.DefaultUserReleaseInvite()
                .WithEmail(user1.Email)
                .WithReleaseVersion(releaseVersion1)
                .WithRole(ReleaseRole.Contributor)
                .Generate();

            var user2 = new User { Email = "test2@test.com" };
            var releaseVersion2 = _fixture.DefaultReleaseVersion()
                    .WithRelease(_fixture.DefaultRelease()
                        .WithPublication(_fixture.DefaultPublication()))
                    .Generate();
            var userReleaseRole2 = _fixture.DefaultUserReleaseRole()
                .WithUser(user2)
                .WithReleaseVersion(releaseVersion2)
                .WithRole(ReleaseRole.PrereleaseViewer)
                .Generate();
            var userReleaseInvite2 = _fixture.DefaultUserReleaseInvite()
                .WithEmail(user2.Email)
                .WithReleaseVersion(releaseVersion2)
                .WithRole(ReleaseRole.PrereleaseViewer)
                .Generate();

            var user3 = new User { Email = "test3@test.com" };
            var releaseVersion3 = _fixture.DefaultReleaseVersion()
                    .WithRelease(_fixture.DefaultRelease()
                        .WithPublication(_fixture.DefaultPublication()))
                    .Generate();
            var userReleaseRole3 = _fixture.DefaultUserReleaseRole()
                .WithUser(user3)
                .WithReleaseVersion(releaseVersion3)
                .WithRole(ReleaseRole.Approver)
                .Generate();
            var userReleaseInvite3 = _fixture.DefaultUserReleaseInvite()
                .WithEmail(user3.Email)
                .WithReleaseVersion(releaseVersion3)
                .WithRole(ReleaseRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRole1, userReleaseRole2, userReleaseRole3);
                contentDbContext.UserReleaseInvites.AddRange(userReleaseInvite1, userReleaseInvite2, userReleaseInvite3);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveMany(
                    It.Is<IReadOnlyList<UserReleaseInvite>>(list =>
                        list.Count == 2
                        && (list[0].Id == userReleaseInvite1.Id
                            ||
                            list[1].Id == userReleaseInvite2.Id)),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await service.RemoveRolesAndInvites([userReleaseRole1, userReleaseRole2]);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRole = await contentDbContext.UserReleaseRoles
                    .SingleAsync();

                Assert.Equal(userReleaseRole3.Id, userReleaseRole.Id);
            }
        }
    }

    public class RemoveAllRolesAndInvitesForPublicationTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task TargetPublicationHasRoles_RemovesTargetRoles()
        {
            var user1 = new User
            {
                Email = "test1@test.com"
            };
            var user2 = new User
            {
                Email = "test2@test.com"
            };
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetPublication = _fixture.DefaultPublication()
                .Generate();
            var otherPublication = _fixture.DefaultPublication()
               .Generate();
            var targetReleaseVersion1 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(otherPublication))
                .Generate();

            var expectedUserReleaseRolesToRemove = new List<UserReleaseRole>();
            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var role in allRoles)
            {
                var targetedUserReleaseRoles = new[] { 
                    // Create a user release role for EACH ROLE for each TARGET release version and EACH EMAIL
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(user1)
                        .WithRole(role)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(user2)
                        .WithRole(role)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(user1)
                        .WithRole(role)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(user2)
                        .WithRole(role)
                        .Generate()
                };

                expectedUserReleaseRolesToRemove.AddRange(targetedUserReleaseRoles);

                allUserReleaseRoles.AddRange([
                    .. targetedUserReleaseRoles,
                    // Create a user release role for EACH ROLE for the OTHER release version and EACH EMAIL
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user1)
                        .WithRole(role)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user2)
                        .WithRole(role)
                        .Generate(),
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByPublication(
                    targetPublication.Id,
                    It.IsAny<CancellationToken>(),
                    new ReleaseRole[] { }))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForPublication(publicationId: targetPublication.Id);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                var expectedNumberOfRolesToRemove = allRoles.Count * 4; // 2 release versions + 2 emails
                var expectedNumberOfRemainingRoles = allUserReleaseRoles.Count - expectedNumberOfRolesToRemove;
                Assert.Equal(expectedNumberOfRemainingRoles, remainingRoles.Count);

                Assert.DoesNotContain(remainingRoles, role =>
                    expectedUserReleaseRolesToRemove.Any(i =>
                        role.ReleaseVersionId == i.ReleaseVersionId &&
                        role.UserId == i.UserId &&
                        role.Role == i.Role));
            }
        }

        [Theory]
        [InlineData(new[] { ReleaseRole.Approver })]
        [InlineData(new[] { ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.PrereleaseViewer })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer })]
        public async Task TargetPublicationAndRolesCombinationHasRoles_RemovesTargetRoles(
            ReleaseRole[] targetRolesToInclude)
        {
            var user1 = new User { Email = "test1@test.com" };
            var user2 = new User { Email = "test2@test.com" };
            var otherRoles = EnumUtil.GetEnums<ReleaseRole>()
                .Except(targetRolesToInclude);
            var targetPublication = _fixture.DefaultPublication()
                .Generate();
            var otherPublication = _fixture.DefaultPublication()
               .Generate();
            var targetReleaseVersion1 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(otherPublication))
                .Generate();

            var expectedUserReleaseRolesToRemove = new List<UserReleaseRole>();
            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var targetRole in targetRolesToInclude)
            {
                var targetedUserReleaseRoles = new[] { 
                    // Create a user release role for each TARGET role for each TARGET release version and EACH EMAIL
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(user1)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(user2)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(user1)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(user2)
                        .WithRole(targetRole)
                        .Generate()
                };

                expectedUserReleaseRolesToRemove.AddRange(targetedUserReleaseRoles);

                allUserReleaseRoles.AddRange([
                    .. targetedUserReleaseRoles,
                    // Create a user release role for each TARGET role for the OTHER release version and EACH EMAIL
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user1)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user2)
                        .WithRole(targetRole)
                        .Generate()
                ]);
            }

            foreach (var otherRole in otherRoles)
            {
                allUserReleaseRoles.AddRange([
                    // Create a user release role for each OTHER role for each TARGET release version and EACH EMAIL
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(user1)
                        .WithRole(otherRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(user2)
                        .WithRole(otherRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(user1)
                        .WithRole(otherRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(user2)
                        .WithRole(otherRole)
                        .Generate(),
                    // Create a user release role for each OTHER role for the OTHER release version and EACH EMAIL
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user1)
                        .WithRole(otherRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user2)
                        .WithRole(otherRole)
                        .Generate()
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByPublication(
                    targetPublication.Id,
                    It.IsAny<CancellationToken>(),
                    targetRolesToInclude))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForPublication(
                    publicationId: targetPublication.Id,
                    rolesToInclude: targetRolesToInclude);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                var expectedNumberOfRolesToRemove = targetRolesToInclude.Length * 4; // 2 release versions + 2 emails
                var expectedNumberOfRemainingRoles = allUserReleaseRoles.Count - expectedNumberOfRolesToRemove;
                Assert.Equal(expectedNumberOfRemainingRoles, remainingRoles.Count);

                Assert.DoesNotContain(remainingRoles, role =>
                    expectedUserReleaseRolesToRemove.Any(i =>
                        role.ReleaseVersionId == i.ReleaseVersionId &&
                        role.UserId == i.UserId &&
                        role.Role == i.Role));
            }
        }

        [Fact]
        public async Task TargetPublicationHasNoRoles_DoesNothing()
        {
            var user1 = new User { Email = "test1@test.com" };
            var user2 = new User { Email = "test2@test.com" };
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetPublication = _fixture.DefaultPublication()
                .Generate();
            var otherPublication = _fixture.DefaultPublication()
               .Generate();
            var targetReleaseVersion1 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(otherPublication))
                .Generate();

            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var role in allRoles)
            {
                allUserReleaseRoles.AddRange([
                    // Create a user release role for EACH ROLE for the OTHER release version and EACH EMAIL
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user1)
                        .WithRole(role)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user2)
                        .WithRole(role)
                        .Generate(),
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByPublication(
                    targetPublication.Id,
                    It.IsAny<CancellationToken>(),
                    new ReleaseRole[] { }))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForPublication(publicationId: targetPublication.Id);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                Assert.Equal(allUserReleaseRoles.Count, remainingRoles.Count);
            }
        }
    }

    public class RemoveAllRolesAndInvitesForPublicationAndUserTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task TargetPublicationAndUserCombinationHasRoles_RemovesTargetRoles()
        {
            var targetUser = new User
            {
                Email = "test1@test.com"
            }; ;
            var otherUser = new User
            {
                Email = "test2@test.com"
            }; ;
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetPublication = _fixture.DefaultPublication()
                .Generate();
            var otherPublication = _fixture.DefaultPublication()
               .Generate();
            var targetReleaseVersion1 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(otherPublication))
                .Generate();

            var expectedUserReleaseRolesToRemove = new List<UserReleaseRole>();
            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var role in allRoles)
            {
                var targetedUserReleaseRoles = new[] { 
                    // Create a user release role for EACH ROLE for each TARGET release version and TARGET email
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(targetUser)
                        .WithRole(role)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(targetUser)
                        .WithRole(role)
                        .Generate()
                };

                expectedUserReleaseRolesToRemove.AddRange(targetedUserReleaseRoles);

                allUserReleaseRoles.AddRange([
                    .. targetedUserReleaseRoles,
                    // Create a user release role for EACH ROLE for each TARGET release version and OTHER email
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(otherUser)
                        .WithRole(role)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(otherUser)
                        .WithRole(role)
                        .Generate(),
                    // Create a user release role for EACH ROLE for the OTHER release version and TARGET email
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(targetUser)
                        .WithRole(role)
                        .Generate(),
                    // Create a user release role for EACH ROLE for the OTHER release version and OTHER email
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(role)
                        .Generate()
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByPublicationAndEmail(
                    targetPublication.Id,
                    targetUser.Email,
                    It.IsAny<CancellationToken>(),
                    new ReleaseRole[] { }))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForPublicationAndUser(
                    publicationId: targetPublication.Id,
                    userId: targetUser.Id);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                var expectedNumberOfRolesToRemove = allRoles.Count * 2; // 2 release versions
                var expectedNumberOfRemainingRoles = allUserReleaseRoles.Count - expectedNumberOfRolesToRemove;
                Assert.Equal(expectedNumberOfRemainingRoles, remainingRoles.Count);

                Assert.DoesNotContain(remainingRoles, role =>
                    expectedUserReleaseRolesToRemove.Any(i =>
                        role.ReleaseVersionId == i.ReleaseVersionId &&
                        role.UserId == i.UserId &&
                        role.Role == i.Role));
            }
        }

        [Theory]
        [InlineData(new[] { ReleaseRole.Approver })]
        [InlineData(new[] { ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.PrereleaseViewer })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer })]
        public async Task TargetPublicationAndUserAndRolesCombinationHasRoles_RemovesTargetRoles(
            ReleaseRole[] targetRolesToInclude)
        {
            var targetUser = new User { Email = "test1@test.com" };
            var otherUser = new User { Email = "test2@test.com" };
            var otherRoles = EnumUtil.GetEnums<ReleaseRole>()
                .Except(targetRolesToInclude);
            var targetPublication = _fixture.DefaultPublication()
                .Generate();
            var otherPublication = _fixture.DefaultPublication()
               .Generate();
            var targetReleaseVersion1 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(otherPublication))
                .Generate();

            var expectedUserReleaseRolesToRemove = new List<UserReleaseRole>();
            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var targetRole in targetRolesToInclude)
            {
                var targetedUserReleaseRoles = new[] { 
                    // Create a user release role for each TARGET role for each TARGET release version and TARGET email
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(targetUser)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(targetUser)
                        .WithRole(targetRole)
                        .Generate()
                };

                expectedUserReleaseRolesToRemove.AddRange(targetedUserReleaseRoles);

                allUserReleaseRoles.AddRange([
                    .. targetedUserReleaseRoles,
                    // Create a user release role for each TARGET role for each TARGET release version and OTHER email
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(otherUser)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(otherUser)
                        .WithRole(targetRole)
                        .Generate(),
                    // Create a user release role for each TARGET role for the OTHER release version and OTHER email
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(targetRole)
                        .Generate()
                ]);
            }

            foreach (var otherRole in otherRoles)
            {
                allUserReleaseRoles.AddRange([
                    // Create a user release role for each OTHER role for each TARGET release version and TARGET email
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(targetUser)
                        .WithRole(otherRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(targetUser)
                        .WithRole(otherRole)
                        .Generate(),
                    // Create a user release role for each OTHER role for each TARGET release version and OTHER email
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithUser(otherUser)
                        .WithRole(otherRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithUser(otherUser)
                        .WithRole(otherRole)
                        .Generate(),
                    // Create a user release role for each OTHER role for the OTHER release version and OTHER email
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(otherRole)
                        .Generate()
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByPublicationAndEmail(
                    targetPublication.Id,
                    targetUser.Email,
                    It.IsAny<CancellationToken>(),
                    targetRolesToInclude))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForPublicationAndUser(
                    publicationId: targetPublication.Id,
                    userId: targetUser.Id,
                    rolesToInclude: targetRolesToInclude);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                .ToListAsync();

                var expectedNumberOfRolesToRemove = targetRolesToInclude.Length * 2; // 2 release versions
                var expectedNumberOfRemainingRoles = allUserReleaseRoles.Count - expectedNumberOfRolesToRemove;
                Assert.Equal(expectedNumberOfRemainingRoles, remainingRoles.Count);

                Assert.DoesNotContain(remainingRoles, role =>
                    expectedUserReleaseRolesToRemove.Any(i =>
                        role.ReleaseVersionId == i.ReleaseVersionId &&
                        role.UserId == i.UserId &&
                        role.Role == i.Role));
            }
        }

        [Fact]
        public async Task TargetPublicationAndUserHasNoRoles_DoesNothing()
        {
            var targetUser = new User { Email = "test1@test.com" };
            var otherUser = new User { Email = "test2@test.com" };
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetPublication = _fixture.DefaultPublication()
                .Generate();
            var otherPublication = _fixture.DefaultPublication()
               .Generate();
            var targetReleaseVersion1 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(otherPublication))
                .Generate();

            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var role in allRoles)
            {
                allUserReleaseRoles.AddRange([
                    // Create a user release role for EACH ROLE for the OTHER release version and OTHER EMAIL
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(role)
                        .Generate()
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                contentDbContext.Users.Add(targetUser);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByPublicationAndEmail(
                    targetPublication.Id,
                    targetUser.Email,
                    It.IsAny<CancellationToken>(),
                    new ReleaseRole[] { }))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForPublicationAndUser(
                    publicationId: targetPublication.Id,
                    userId: targetUser.Id);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                Assert.Equal(allUserReleaseRoles.Count, remainingRoles.Count);
            }
        }

        [Fact]
        public async Task UserIsEmpty_ThrowsException()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var manager = SetupUserReleaseRoleAndInviteManager(
                contentDbContext: contentDbContext);

            await Assert.ThrowsAsync<ArgumentException>(
                async () => await manager.RemoveAllRolesAndInvitesForPublicationAndUser(
                    publicationId: Guid.NewGuid(),
                    userId: Guid.Empty));
        }

        [Fact]
        public async Task UserDoesNotExist_ThrowsException()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var manager = SetupUserReleaseRoleAndInviteManager(
                contentDbContext: contentDbContext);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await manager.RemoveAllRolesAndInvitesForPublicationAndUser(
                    publicationId: Guid.NewGuid(),
                    userId: Guid.NewGuid()));
        }
    }

    public class RemoveAllRolesAndInvitesForReleaseVersionTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task TargetReleaseVersionHasRoles_RemovesTargetRoles()
        {
            var user1 = new User { Email = "test1@test.com" };
            var user2 = new User { Email = "test2@test.com" };
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var expectedUserReleaseRolesToRemove = new List<UserReleaseRole>();
            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var role in allRoles)
            {
                var targetedUserReleaseRoles = new[] { 
                    // Create a user release role for EACH ROLE for the TARGET release version and EACH user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(user1)
                        .WithRole(role)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(user2)
                        .WithRole(role)
                        .Generate()
                };

                expectedUserReleaseRolesToRemove.AddRange(targetedUserReleaseRoles);

                allUserReleaseRoles.AddRange([
                    .. targetedUserReleaseRoles,
                    // Create a user release role for EACH ROLE for the OTHER release version and EACH user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user1)
                        .WithRole(role)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user2)
                        .WithRole(role)
                        .Generate(),
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByReleaseVersion(
                    targetReleaseVersion.Id,
                    It.IsAny<CancellationToken>(),
                    new ReleaseRole[] { }))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForReleaseVersion(releaseVersionId: targetReleaseVersion.Id);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                var expectedNumberOfRolesToRemove = allRoles.Count * 2; // 2 users
                var expectedNumberOfRemainingRoles = allUserReleaseRoles.Count - expectedNumberOfRolesToRemove;
                Assert.Equal(expectedNumberOfRemainingRoles, remainingRoles.Count);

                Assert.DoesNotContain(remainingRoles, role =>
                    expectedUserReleaseRolesToRemove.Any(i =>
                        role.ReleaseVersionId == i.ReleaseVersionId &&
                        role.UserId == i.UserId &&
                        role.Role == i.Role));
            }
        }

        [Theory]
        [InlineData(new[] { ReleaseRole.Approver })]
        [InlineData(new[] { ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.PrereleaseViewer })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer })]
        public async Task TargetReleaseVersionAndRolesCombinationHasRoles_RemovesTargetRoles(
            ReleaseRole[] targetRolesToInclude)
        {
            var user1 = new User { Email = "test1@test.com" };
            var user2 = new User { Email = "test2@test.com" };
            var otherRoles = EnumUtil.GetEnums<ReleaseRole>()
                .Except(targetRolesToInclude);
            var targetReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var expectedUserReleaseRolesToRemove = new List<UserReleaseRole>();
            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var targetRole in targetRolesToInclude)
            {
                var targetedUserReleaseRoles = new[] { 
                    // Create a user release role for each TARGET role for the TARGET release version and EACH user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(user1)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(user2)
                        .WithRole(targetRole)
                        .Generate(),
                };

                expectedUserReleaseRolesToRemove.AddRange(targetedUserReleaseRoles);

                allUserReleaseRoles.AddRange([
                    .. targetedUserReleaseRoles,
                    // Create a user release role for each TARGET role for the OTHER release version and EACH user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user1)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user2)
                        .WithRole(targetRole)
                        .Generate()
                ]);
            }

            foreach (var otherRole in otherRoles)
            {
                allUserReleaseRoles.AddRange([
                    // Create a user release role for each OTHER role for the TARGET release version and EACH user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(user1)
                        .WithRole(otherRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(user2)
                        .WithRole(otherRole)
                        .Generate(),
                    // Create a user release role for each OTHER role for the OTHER release version and EACH user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user1)
                        .WithRole(otherRole)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user2)
                        .WithRole(otherRole)
                        .Generate()
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByReleaseVersion(
                    targetReleaseVersion.Id,
                    It.IsAny<CancellationToken>(),
                    targetRolesToInclude))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForReleaseVersion(
                    releaseVersionId: targetReleaseVersion.Id,
                    rolesToInclude: targetRolesToInclude);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                var expectedNumberOfRolesToRemove = targetRolesToInclude.Length * 2; // 2 users
                var expectedNumberOfRemainingRoles = allUserReleaseRoles.Count - expectedNumberOfRolesToRemove;
                Assert.Equal(expectedNumberOfRemainingRoles, remainingRoles.Count);

                Assert.DoesNotContain(remainingRoles, role =>
                    expectedUserReleaseRolesToRemove.Any(i =>
                        role.ReleaseVersionId == i.ReleaseVersionId &&
                        role.UserId == i.UserId &&
                        role.Role == i.Role));
            }
        }

        [Fact]
        public async Task TargetReleaseVersionHasNoRoles_DoesNothing()
        {
            var user1 = new User { Email = "test1@test.com" };
            var user2 = new User { Email = "test2@test.com" };
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var role in allRoles)
            {
                allUserReleaseRoles.AddRange([
                    // Create a user release role for EACH ROLE for the OTHER release version and EACH user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user1)
                        .WithRole(role)
                        .Generate(),
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(user2)
                        .WithRole(role)
                        .Generate(),
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByReleaseVersion(
                    targetReleaseVersion.Id,
                    It.IsAny<CancellationToken>(),
                    new ReleaseRole[] { }))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForReleaseVersion(releaseVersionId: targetReleaseVersion.Id);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                Assert.Equal(allUserReleaseRoles.Count, remainingRoles.Count);
            }
        }
    }

    public class RemoveAllRolesAndInvitesForReleaseVersionAndUserTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task TargetReleaseVersionAndUserCombinationHasRoles_RemovesTargetRoles()
        {
            var targetUser = new User { Email = "test1@test.com" };
            var otherUser = new User { Email = "test2@test.com" };
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var expectedUserReleaseRolesToRemove = new List<UserReleaseRole>();
            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var role in allRoles)
            {
                var targetedUserReleaseRoles = new[] { 
                    // Create a user release role for EACH ROLE for the TARGET release version and TARGET user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(targetUser)
                        .WithRole(role)
                        .Generate()
                };

                expectedUserReleaseRolesToRemove.AddRange(targetedUserReleaseRoles);

                allUserReleaseRoles.AddRange([
                    .. targetedUserReleaseRoles,
                    // Create a user release role for EACH ROLE for the TARGET release version and OTHER user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(role)
                        .Generate(),
                    // Create a user release role for EACH ROLE for the OTHER release version and TARGET user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(targetUser)
                        .WithRole(role)
                        .Generate(),
                    // Create a user release role for EACH ROLE for the OTHER release version and OTHER user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(role)
                        .Generate()
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByReleaseVersionAndEmail(
                    targetReleaseVersion.Id,
                    targetUser.Email,
                    It.IsAny<CancellationToken>(),
                    new ReleaseRole[] { }))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForReleaseVersionAndUser(
                    releaseVersionId: targetReleaseVersion.Id,
                    userId: targetUser.Id);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                var expectedNumberOfRolesToRemove = allRoles.Count;
                var expectedNumberOfRemainingRoles = allUserReleaseRoles.Count - expectedNumberOfRolesToRemove;
                Assert.Equal(expectedNumberOfRemainingRoles, remainingRoles.Count);

                Assert.DoesNotContain(remainingRoles, role =>
                    expectedUserReleaseRolesToRemove.Any(i =>
                        role.ReleaseVersionId == i.ReleaseVersionId &&
                        role.UserId == i.UserId &&
                        role.Role == i.Role));
            }
        }

        [Theory]
        [InlineData(new[] { ReleaseRole.Approver })]
        [InlineData(new[] { ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.PrereleaseViewer })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer })]
        public async Task TargetReleaseVersionAndUserAndRolesCombinationHasRoles_RemovesTargetRoles(
            ReleaseRole[] targetRolesToInclude)
        {
            var targetUser = new User { Email = "test1@test.com" };
            var otherUser = new User { Email = "test2@test.com" };
            var otherRoles = EnumUtil.GetEnums<ReleaseRole>()
                .Except(targetRolesToInclude);
            var targetReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var expectedUserReleaseRolesToRemove = new List<UserReleaseRole>();
            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var targetRole in targetRolesToInclude)
            {
                var targetedUserReleaseRoles = new[] { 
                    // Create a user release role for each TARGET role for the TARGET release version and TARGET user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(targetUser)
                        .WithRole(targetRole)
                        .Generate(),
                };

                expectedUserReleaseRolesToRemove.AddRange(targetedUserReleaseRoles);

                allUserReleaseRoles.AddRange([
                    .. targetedUserReleaseRoles,
                    // Create a user release role for each TARGET role for the TARGET release version and OTHER user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(targetRole)
                        .Generate(),
                    // Create a user release role for each TARGET role for the OTHER release version and OTHER user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(targetRole)
                        .Generate()
                ]);
            }

            foreach (var otherRole in otherRoles)
            {
                allUserReleaseRoles.AddRange([
                    // Create a user release role for each OTHER role for the TARGET release version and TARGET user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(targetUser)
                        .WithRole(otherRole)
                        .Generate(),
                    // Create a user release role for each OTHER role for the TARGET release version and OTHER user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(otherRole)
                        .Generate(),
                    // Create a user release role for each OTHER role for the OTHER release version and OTHER user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(otherRole)
                        .Generate()
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByReleaseVersionAndEmail(
                    targetReleaseVersion.Id,
                    targetUser.Email,
                    It.IsAny<CancellationToken>(),
                    targetRolesToInclude))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForReleaseVersionAndUser(
                    releaseVersionId: targetReleaseVersion.Id,
                    userId: targetUser.Id,
                    rolesToInclude: targetRolesToInclude);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                var expectedNumberOfRolesToRemove = targetRolesToInclude.Length;
                var expectedNumberOfRemainingRoles = allUserReleaseRoles.Count - expectedNumberOfRolesToRemove;
                Assert.Equal(expectedNumberOfRemainingRoles, remainingRoles.Count);

                Assert.DoesNotContain(remainingRoles, role =>
                    expectedUserReleaseRolesToRemove.Any(i =>
                        role.ReleaseVersionId == i.ReleaseVersionId &&
                        role.UserId == i.UserId &&
                        role.Role == i.Role));
            }
        }

        [Fact]
        public async Task TargetReleaseVersionAndUserHasNoRoles_DoesNothing()
        {
            var targetUser = new User { Email = "test1@test.com" };
            var otherUser = new User { Email = "test2@test.com" };
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var allUserReleaseRoles = new List<UserReleaseRole>();

            foreach (var role in allRoles)
            {
                allUserReleaseRoles.AddRange([
                    // Create a user release role for EACH ROLE for the OTHER release version and OTHER user
                    _fixture.DefaultUserReleaseRole()
                        .WithReleaseVersion(otherReleaseVersion)
                        .WithUser(otherUser)
                        .WithRole(role)
                        .Generate()
                ]);
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
                contentDbContext.Users.Add(targetUser);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByReleaseVersionAndEmail(
                    targetReleaseVersion.Id,
                    targetUser.Email,
                    It.IsAny<CancellationToken>(),
                    new ReleaseRole[] { }))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForReleaseVersionAndUser(
                    releaseVersionId: targetReleaseVersion.Id,
                    userId: targetUser.Id);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .ToListAsync();

                Assert.Equal(allUserReleaseRoles.Count, remainingRoles.Count);
            }
        }

        [Fact]
        public async Task UserIsEmpty_ThrowsException()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var manager = SetupUserReleaseRoleAndInviteManager(
                contentDbContext: contentDbContext);

            await Assert.ThrowsAsync<ArgumentException>(
                async () => await manager.RemoveAllRolesAndInvitesForReleaseVersionAndUser(
                    releaseVersionId: Guid.NewGuid(),
                    userId: Guid.Empty));
        }

        [Fact]
        public async Task UserDoesNotExist_ThrowsException()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var manager = SetupUserReleaseRoleAndInviteManager(
                contentDbContext: contentDbContext);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await manager.RemoveAllRolesAndInvitesForReleaseVersionAndUser(
                    releaseVersionId: Guid.NewGuid(),
                    userId: Guid.NewGuid()));
        }
    }

    public class RemoveAllRolesAndInvitesForUserTests : UserReleaseRoleAndInviteManagerTests
    {
        [Fact]
        public async Task TargetUserHasRoles_RemovesTargetRoles()
        {
            var targetUser = new User { Email = "test1@test.com" };
            var otherUser = new User { Email = "test2@test.com" };
            var role1 = ReleaseRole.Approver;
            var role2 = ReleaseRole.Contributor;
            var releaseVersion1 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var releaseVersion2 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var userReleaseRoles = _fixture.DefaultUserReleaseRole()
                // These 2 roles should be removed
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(0, s => s.SetUser(targetUser))
                .ForIndex(0, s => s.SetRole(role1))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(1, s => s.SetUser(targetUser))
                .ForIndex(1, s => s.SetRole(role2))
                // These roles are for a different email and should not be removed
                .ForIndex(2, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(2, s => s.SetUser(otherUser))
                .ForIndex(2, s => s.SetRole(role1))
                .ForIndex(3, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(3, s => s.SetUser(otherUser))
                .ForIndex(3, s => s.SetRole(role2))
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByUserEmail(
                    targetUser.Email,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForUser(targetUser.Id);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .Include(urr => urr.User)
                    .ToListAsync();

                Assert.Equal(2, remainingRoles.Count);

                Assert.Equal(releaseVersion1.Id, remainingRoles[0].ReleaseVersionId);
                Assert.Equal(otherUser.Id, remainingRoles[0].User.Id);
                Assert.Equal(role1, remainingRoles[0].Role);

                Assert.Equal(releaseVersion2.Id, remainingRoles[1].ReleaseVersionId);
                Assert.Equal(otherUser.Id, remainingRoles[1].User.Id);
                Assert.Equal(role2, remainingRoles[1].Role);
            }
        }

        [Fact]
        public async Task TargetUserHasNoRoles_DoesNothing()
        {
            var targetUser = new User { Email = "test1@test.com" };
            var otherUser = new User { Email = "test2@test.com" };
            var role1 = ReleaseRole.Approver;
            var role2 = ReleaseRole.Contributor;
            var releaseVersion1 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var releaseVersion2 = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var userReleaseRoles = _fixture.DefaultUserReleaseRole()
                // These roles are for a different email and should not be removed
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(0, s => s.SetUser(otherUser))
                .ForIndex(0, s => s.SetRole(role1))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(1, s => s.SetUser(otherUser))
                .ForIndex(1, s => s.SetRole(role2))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(targetUser, otherUser);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m => m.RemoveByUserEmail(
                    targetUser.Email,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = SetupUserReleaseRoleAndInviteManager(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                await manager.RemoveAllRolesAndInvitesForUser(targetUser.Id);
            }

            MockUtils.VerifyAllMocks(userReleaseInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles
                    .Include(urr => urr.User)
                    .ToListAsync();

                Assert.Equal(2, remainingRoles.Count);

                Assert.Equal(releaseVersion1.Id, remainingRoles[0].ReleaseVersionId);
                Assert.Equal(otherUser.Id, remainingRoles[0].User.Id);
                Assert.Equal(role1, remainingRoles[0].Role);

                Assert.Equal(releaseVersion2.Id, remainingRoles[1].ReleaseVersionId);
                Assert.Equal(otherUser.Id, remainingRoles[1].User.Id);
                Assert.Equal(role2, remainingRoles[1].Role);
            }
        }
    }

    private static UserReleaseRoleAndInviteManager SetupUserReleaseRoleAndInviteManager(
        ContentDbContext contentDbContext,
        IUserReleaseInviteRepository? userReleaseInviteRepository = null)
    {
        return new(
            contentDbContext: contentDbContext,
            userReleaseInviteRepository: userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(),
            userRepository: new UserRepository(contentDbContext),
            logger: Mock.Of<ILogger<UserReleaseRoleAndInviteManager>>());
    }
}
