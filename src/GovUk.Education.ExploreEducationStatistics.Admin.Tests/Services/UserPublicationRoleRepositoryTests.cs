#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task Create()
        {
            var user = new User();

            var createdBy = new User();

            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddRangeAsync(user, createdBy);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserPublicationRoleRepository(contentDbContext);

                var result = await service.Create(user.Id, publication.Id, Owner, createdBy.Id);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(Owner, result.Role);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(createdBy.Id, result.CreatedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();
                Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, userPublicationRoles[0].Id);
                Assert.Equal(user.Id, userPublicationRoles[0].UserId);
                Assert.Equal(publication.Id, userPublicationRoles[0].PublicationId);
                Assert.Equal(Owner, userPublicationRoles[0].Role);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationRoles[0].Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(createdBy.Id, userPublicationRoles[0].CreatedById);
            }
        }
        
        [Fact]
        public async Task GetDistinctRolesByUser()
        {
            var user = new User();
            var publication1 = new Publication();
            var publication2 = new Publication();

            var userPublicationRoles = new List<UserPublicationRole>
            {
                new()
                {
                    User = user,
                    Publication = publication1,
                    Role = Owner
                },
                new()
                {
                    User = user,
                    Publication = publication2,
                    Role = Owner
                }
            };

            var otherUserPublicationRoles = new List<UserPublicationRole>
            {
                // Role for different user
                new()
                {
                    User = new User(),
                    Publication = publication1,
                    Role = Owner
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(user);
                await contentDbContext.AddRangeAsync(publication1, publication2);
                await contentDbContext.AddRangeAsync(userPublicationRoles);
                await contentDbContext.AddRangeAsync(otherUserPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserPublicationRoleRepository(contentDbContext);

                var result = await service.GetDistinctRolesByUser(user.Id);

                // Expect only distinct roles to be returned, therefore the 2nd "Owner" role is filtered out.
                Assert.Single(result);
                Assert.Equal(Owner, result[0]);
            }
        }

        [Fact]
        public async Task UserHasRoleOnPublication_TrueIfRoleExists()
        {
            var userPublicationRole = new UserPublicationRole
            {
                User = new User(),
                Publication = new Publication(),
                Role = Owner
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserPublicationRoleRepository(contentDbContext);

                Assert.True(await service.UserHasRoleOnPublication(
                    userPublicationRole.UserId,
                    userPublicationRole.PublicationId,
                    Owner));
            }
        }

        [Fact]
        public async Task UserHasRoleOnPublication_FalseIfRoleDoesNotExist()
        {
            var publication = new Publication();

            // Setup a role but for a different publication to make sure it has no influence
            var userPublicationRoleOtherPublication = new UserPublicationRole
            {
                User = new User(),
                Publication = new Publication(),
                Role = Owner
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRoleOtherPublication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserPublicationRoleRepository(contentDbContext);

                Assert.False(await service.UserHasRoleOnPublication(
                    userPublicationRoleOtherPublication.UserId,
                    publication.Id,
                    Owner));
            }
        }

        private static UserPublicationRoleRepository SetupUserPublicationRoleRepository(
            ContentDbContext contentDbContext)
        {
            return new(contentDbContext);
        }
    }
}
