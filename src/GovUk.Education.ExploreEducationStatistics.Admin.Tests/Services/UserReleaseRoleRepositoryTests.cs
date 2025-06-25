#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserReleaseRoleRepositoryTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task Create_NoNewPermissionsSystemPublicationRoleChanges()
        {
            var releaseRoleToCreate = ReleaseRole.Contributor;
            var newPublicationRoleToRemain = PublicationRole.Approver;

            var user = new User();
            var createdBy = new User();
            var publication = _fixture.DefaultPublication()
                .Generate();
            var releaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(publication))
                .Generate();
            var existingPublicationRole = _fixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(newPublicationRoleToRemain)
                .Generate();

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
            await contentDbContext.SaveChangesAsync();

            var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
            newPermissionsSystemHelperMock
                .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                    releaseRoleToCreate,
                    user.Id,
                    releaseVersion.Release.PublicationId))
                .ReturnsAsync((null, null));

            var service = SetupUserReleaseRoleRepository(contentDbContext, newPermissionsSystemHelperMock.Object);

            var result = await service.Create(
                userId: user.Id,
                releaseVersionId: releaseVersion.Id,
                releaseRole: releaseRoleToCreate,
                createdById: createdBy.Id);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
            Assert.Equal(releaseRoleToCreate, result.Role);
            Assert.Equal(createdBy.Id, result.CreatedById);
            result.Created.AssertUtcNow();

            var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

            // Should have created the `Contributor` release role
            var createdReleaseRole = Assert.Single(userReleaseRoles);

            Assert.NotEqual(Guid.Empty, createdReleaseRole.Id);
            Assert.Equal(user.Id, createdReleaseRole.UserId);
            Assert.Equal(releaseVersion.Id, createdReleaseRole.ReleaseVersionId);
            Assert.Equal(releaseRoleToCreate, createdReleaseRole.Role);
            Assert.Equal(createdBy.Id, createdReleaseRole.CreatedById);

            var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

            // Should not have created any new publication roles
            Assert.Empty(userPublicationRoles);
        }

        [Fact]
        public async Task Create_NewPermissionsSystemPublicationRolesToRemoveAndCreateFromOldRole()
        {
            var releaseRoleToCreate = ReleaseRole.Approver;
            var newPublicationRoleToCreate = PublicationRole.Approver;
            var newPublicationRoleToRemove = PublicationRole.Drafter;

            var user = new User();
            var createdBy = new User();
            var publication = _fixture.DefaultPublication()
                .Generate();
            var releaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(publication))
                .Generate();
            var existingPublicationRole = _fixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(newPublicationRoleToRemove)
                .Generate();

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
            await contentDbContext.SaveChangesAsync();

            var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
            newPermissionsSystemHelperMock
                .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                    releaseRoleToCreate,
                    user.Id,
                    publication.Id))
                .ReturnsAsync((newPublicationRoleToRemove, newPublicationRoleToCreate));

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.GetUserPublicationRole(
                    user.Id,
                    publication.Id,
                    newPublicationRoleToRemove))
                .ReturnsAsync(existingPublicationRole);
            //userPublicationRoleRepositoryMock
            //    .Setup(rvr => rvr.Remove(
            //        existingPublicationRole,
            //        createdBy.Id));
            //userPublicationRoleRepositoryMock
            //    .Setup(rvr => rvr.TryCreate(
            //        user.Id,
            //        publication.Id,
            //        newPublicationRoleToCreate,
            //        createdBy.Id));

            var service = SetupUserReleaseRoleRepository(
                contentDbContext: contentDbContext,
                newPermissionsSystemHelper: newPermissionsSystemHelperMock.Object,
                userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object);

            var result = await service.Create(
                userId: user.Id,
                releaseVersionId: releaseVersion.Id,
                releaseRole: releaseRoleToCreate,
                createdById: createdBy.Id);

            Assert.NotNull(result);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
            Assert.Equal(releaseRoleToCreate, result.Role);
            result.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, result.CreatedById);

            var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

            // Should have created the `Approver` release role
            var createdReleaseRole = Assert.Single(userReleaseRoles);

            Assert.NotEqual(Guid.Empty, createdReleaseRole.Id);
            Assert.Equal(user.Id, createdReleaseRole.UserId);
            Assert.Equal(releaseVersion.Id, createdReleaseRole.ReleaseVersionId);
            Assert.Equal(releaseRoleToCreate, createdReleaseRole.Role);
            Assert.Equal(createdBy.Id, createdReleaseRole.CreatedById);

            var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

            // Should just be 1 as the the 'Approver` role should have been created,
            // and the 'Drafter` role should have been removed.
            var createdNewPublicationRole = Assert.Single(userPublicationRoles);

            Assert.NotEqual(Guid.Empty, createdNewPublicationRole.Id);
            Assert.Equal(user.Id, createdNewPublicationRole.UserId);
            Assert.Equal(publication.Id, createdNewPublicationRole.PublicationId);
            Assert.Equal(newPublicationRoleToCreate, createdNewPublicationRole.Role);
            createdNewPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, createdNewPublicationRole.CreatedById);
        }

        [Fact]
        public async Task Create_NewPermissionsSystemPublicationRoleToCreate()
        {
            var releaseRoleToCreate = ReleaseRole.Contributor;
            var newPublicationRoleToCreate = PublicationRole.Drafter;

            var user = new User();
            var createdBy = new User();
            var publication = _fixture.DefaultPublication()
                .Generate();
            var releaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(publication))
                .Generate();

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.Users.AddRange(user, createdBy);
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();

            var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
            newPermissionsSystemHelperMock
                .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                    releaseRoleToCreate,
                    user.Id,
                    publication.Id))
                .ReturnsAsync((null, newPublicationRoleToCreate));

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
            //userPublicationRoleRepositoryMock
            //    .Setup(rvr => rvr.TryCreate(
            //        user.Id,
            //        publication.Id,
            //        newPublicationRoleToCreate,
            //        createdBy.Id));

            var service = SetupUserReleaseRoleRepository(
                contentDbContext: contentDbContext,
                newPermissionsSystemHelper: newPermissionsSystemHelperMock.Object,
                userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object);

            var result = await service.Create(
                userId: user.Id,
                releaseVersionId: releaseVersion.Id,
                releaseRole: releaseRoleToCreate,
                createdById: createdBy.Id);

            Assert.NotNull(result);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
            Assert.Equal(releaseRoleToCreate, result.Role);
            result.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, result.CreatedById);

            var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

            // Should have created the `Contributor` release role
            var createdReleaseRole = Assert.Single(userReleaseRoles);

            Assert.NotEqual(Guid.Empty, createdReleaseRole.Id);
            Assert.Equal(user.Id, createdReleaseRole.UserId);
            Assert.Equal(releaseVersion.Id, createdReleaseRole.ReleaseVersionId);
            Assert.Equal(releaseRoleToCreate, createdReleaseRole.Role);
            Assert.Equal(createdBy.Id, createdReleaseRole.CreatedById);

            var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

            // Should just be 1 as the the 'Drafter` role should have been created
            var createdNewPublicationRole = Assert.Single(userPublicationRoles);

            Assert.NotEqual(Guid.Empty, createdNewPublicationRole.Id);
            Assert.Equal(user.Id, createdNewPublicationRole.UserId);
            Assert.Equal(publication.Id, createdNewPublicationRole.PublicationId);
            Assert.Equal(newPublicationRoleToCreate, createdNewPublicationRole.Role);
            createdNewPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, createdNewPublicationRole.CreatedById);
        }

        [Fact]
        public async Task CreateIfNotExists_DoesNotExist()
        {
            var userId = Guid.NewGuid();
            var createdById = Guid.NewGuid();
            var releaseVersionId = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

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
                var service = SetupUserReleaseRoleRepository(contentDbContext);

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
                var service = SetupUserReleaseRoleRepository(contentDbContext);

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
                var service = SetupUserReleaseRoleRepository(contentDbContext);

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
                var service = SetupUserReleaseRoleRepository(contentDbContext);

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

        [Fact]
        public async Task Remove()
        {
            var deletedById = Guid.NewGuid();
            var userReleaseRole = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Approver,
                Deleted = null,
                DeletedById = null,
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
                await service.Remove(userReleaseRole, deletedById);

                var updatedReleaseRole = contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .SingleOrDefault(urr => urr.Id == userReleaseRole.Id);

                Assert.Null(updatedReleaseRole);
            }
        }

        [Fact]
        public async Task Remove_IgnoreQueryFilters()
        {
            var deletedById = Guid.NewGuid();
            var userReleaseRole = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Approver,
                Deleted = null,
                DeletedById = null,
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
                await service.Remove(userReleaseRole, deletedById);

                var updatedReleaseRole = contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .IgnoreQueryFilters()
                    .SingleOrDefault(urr => urr.Id == userReleaseRole.Id);

                Assert.NotNull(updatedReleaseRole);
                Assert.Equal(userReleaseRole.ReleaseVersionId, updatedReleaseRole!.ReleaseVersionId);
                Assert.Equal(userReleaseRole.Role, updatedReleaseRole.Role);
                updatedReleaseRole.Deleted.AssertUtcNow();
                Assert.Equal(deletedById, updatedReleaseRole.DeletedById);
            }
        }

        [Fact]
        public async Task RemoveMany()
        {
            var deletedById = Guid.NewGuid();

            var userReleaseRole1 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
                Deleted = null,
                DeletedById = null,
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.PrereleaseViewer,
                Deleted = null,
                DeletedById = null,
            };
            var notDeletedUserReleaseRole = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Approver,
                Deleted = null,
                DeletedById = null,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userReleaseRole1, userReleaseRole2,
                    notDeletedUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);
                await service.RemoveMany(
                    [userReleaseRole1, userReleaseRole2],
                    deletedById);

                var userReleaseRole = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .SingleAsync();
                Assert.Equal(notDeletedUserReleaseRole.Id, userReleaseRole.Id);
            }
        }

        [Fact]
        public async Task RemoveMany_IgnoreQueryFilters()
        {
            var deletedById = Guid.NewGuid();

            var userReleaseRole1 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
                Deleted = null,
                DeletedById = null,
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.PrereleaseViewer,
                Deleted = null,
                DeletedById = null,
            };
            var notDeletedUserReleaseRole = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Approver,
                Deleted = null,
                DeletedById = null,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userReleaseRole1, userReleaseRole2,
                    notDeletedUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);
                await service.RemoveMany(
                    [userReleaseRole1, userReleaseRole2],
                    deletedById);

                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .IgnoreQueryFilters()
                    .ToListAsync();
                Assert.Equal(3, userReleaseRoles.Count);

                Assert.Equal(userReleaseRole1.Id, userReleaseRoles[0].Id);
                Assert.Equal(userReleaseRole1.ReleaseVersionId, userReleaseRoles[0].ReleaseVersionId);
                Assert.Equal(userReleaseRole1.Role, userReleaseRoles[0].Role);
                userReleaseRoles[0].Deleted.AssertUtcNow();
                Assert.Equal(deletedById, userReleaseRoles[0].DeletedById);

                Assert.Equal(userReleaseRole2.Id, userReleaseRoles[1].Id);
                Assert.Equal(userReleaseRole2.ReleaseVersionId, userReleaseRoles[1].ReleaseVersionId);
                Assert.Equal(userReleaseRole2.Role, userReleaseRoles[1].Role);
                userReleaseRoles[1].Deleted.AssertUtcNow();
                Assert.Equal(deletedById, userReleaseRoles[1].DeletedById);

                Assert.Equal(notDeletedUserReleaseRole.Id, userReleaseRoles[2].Id);
                Assert.Equal(notDeletedUserReleaseRole.ReleaseVersionId, userReleaseRoles[2].ReleaseVersionId);
                Assert.Equal(notDeletedUserReleaseRole.Role, userReleaseRoles[2].Role);
                Assert.Null(userReleaseRoles[2].Deleted);
                Assert.Null(userReleaseRoles[2].DeletedById);
            }
        }

        [Fact]
        public async Task RemoveAllForPublication()
        {
            var user = new User();
            var deletedById = Guid.NewGuid();

            var publication = new Publication
            {
                ReleaseVersions = new List<ReleaseVersion>
                {
                    new() { Id = Guid.NewGuid(), },
                    new() { Id = Guid.NewGuid(), },
                    new() { Id = Guid.NewGuid(), },
                }
            };
            var publication2 = new Publication
            {
                ReleaseVersions = new List<ReleaseVersion>
                {
                    new() { Id = Guid.NewGuid(), }
                }
            };

            var userReleaseRole1 = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = publication.ReleaseVersions[0],
                Role = ReleaseRole.Contributor,
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = publication.ReleaseVersions[2],
                Role = ReleaseRole.Contributor,
            };
            var notDeletedUserReleaseRole1 = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = publication.ReleaseVersions[0],
                Role = ReleaseRole.PrereleaseViewer,
            };
            var notDeletedUserReleaseRole2 = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = publication2.ReleaseVersions[0],
                Role = ReleaseRole.Contributor,
            };
            var notDeletedUserReleaseRole3 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseVersion = publication.ReleaseVersions[0],
                Role = ReleaseRole.Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(publication, publication2,
                    userReleaseRole1, userReleaseRole2,
                    notDeletedUserReleaseRole1, notDeletedUserReleaseRole2, notDeletedUserReleaseRole3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);
                await service.RemoveAllForPublication(user.Id, publication, ReleaseRole.Contributor,
                    deletedById);

                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(3, userReleaseRoles.Count);

                Assert.Equal(notDeletedUserReleaseRole1.Id, userReleaseRoles[0].Id);
                Assert.Equal(notDeletedUserReleaseRole2.Id, userReleaseRoles[1].Id);
                Assert.Equal(notDeletedUserReleaseRole3.Id, userReleaseRoles[2].Id);
            }
        }

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
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.GetAllRolesByUserAndRelease(userId: user.Id,
                    releaseVersionId: releaseVersion.Id);

                Assert.Equal(2, result.Count);
                Assert.Equal(ReleaseRole.Contributor, result[0]);
                Assert.Equal(ReleaseRole.PrereleaseViewer, result[1]);
            }
        }

        [Fact]
        public async Task GetAllRolesByUserAndPublication()
        {
            var user = new User();
            var publication = new Publication();

            var userReleaseRolesForPublication = new List<UserReleaseRole>
            {
                new()
                {
                    User = user,
                    ReleaseVersion = new ReleaseVersion
                    {
                        Publication = publication
                    },
                    Role = ReleaseRole.Contributor
                },
                new()
                {
                    User = user,
                    ReleaseVersion = new ReleaseVersion
                    {
                        Publication = publication
                    },
                    Role = ReleaseRole.Approver
                },
                // Add a duplicate ReleaseRole to ensure duplicates are removed by the method under test
                new()
                {
                    User = user,
                    ReleaseVersion = new ReleaseVersion
                    {
                        Publication = publication
                    },
                    Role = ReleaseRole.Approver
                }
            };

            var otherPublication = new Publication();
            var otherUserReleaseRoles = new List<UserReleaseRole>
            {
                // Role for different Publication
                new()
                {
                    User = user,
                    ReleaseVersion = new ReleaseVersion
                    {
                        Publication = otherPublication
                    },
                    Role = ReleaseRole.PrereleaseViewer
                },
                // Role for same Publication but different user
                new()
                {
                    User = new User(),
                    ReleaseVersion = new ReleaseVersion
                    {
                        Publication = publication
                    },
                    Role = ReleaseRole.PrereleaseViewer
                }
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
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.GetAllRolesByUserAndPublication(user.Id, publication.Id);

                Assert.Equal(2, result.Count);
                Assert.Equal(ReleaseRole.Contributor, result[0]);
                Assert.Equal(ReleaseRole.Approver, result[1]);
            }
        }

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
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.GetDistinctRolesByUser(user.Id);

                // Expect 3 distinct results.  The 4th duplicate "ReleaseRole.PrereleaseViewer" role is filtered out.
                Assert.Equal(3, result.Count);
                Assert.Equal(ReleaseRole.Contributor, result[0]);
                Assert.Equal(ReleaseRole.PrereleaseViewer, result[1]);
                Assert.Equal(ReleaseRole.Approver, result[2]);
            }
        }

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
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.GetUserReleaseRole(
                    userId: userReleaseRole.UserId,
                    releaseVersionId: userReleaseRole.ReleaseVersionId,
                    ReleaseRole.Contributor);

                Assert.NotNull(result);
                Assert.Equal(userReleaseRole.UserId, result!.UserId);
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
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.GetUserReleaseRole(
                    userId: user.Id,
                    releaseVersionId: releaseVersion.Id,
                    ReleaseRole.Contributor);

                Assert.Null(result);
            }
        }

        private static UserReleaseRoleRepository SetupUserReleaseRoleRepository(
            ContentDbContext contentDbContext,
            INewPermissionsSystemHelper? newPermissionsSystemHelper = null,
            IUserPublicationRoleRepository? userPublicationRoleRepository = null)
        {
            var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
            newPermissionsSystemHelperMock
                .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                    It.IsAny<PublicationRole>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>()))
                .ReturnsAsync((null, null));

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();

            return new(
                contentDbContext,
                newPermissionsSystemHelper ?? newPermissionsSystemHelperMock.Object,
                userPublicationRoleRepository ?? userPublicationRoleRepositoryMock.Object);
        }
    }
}
