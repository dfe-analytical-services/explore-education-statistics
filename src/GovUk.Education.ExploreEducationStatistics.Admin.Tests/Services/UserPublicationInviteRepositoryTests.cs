#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserPublicationInviteRepositoryTests
    {
        [Fact]
        public async Task CreateManyIfNotExists()
        {
            var createdById = Guid.NewGuid();
            var userPublicationRole1 = new UserPublicationRoleCreateRequest
            {
                PublicationId = Guid.NewGuid(),
                PublicationRole = Owner,
            };
            var userPublicationRole2 = new UserPublicationRoleCreateRequest
            {
                PublicationId = Guid.NewGuid(),
                PublicationRole = Approver,
            };
            var existingPublicationInvite = new UserPublicationInvite
            {
                Email = "test@test.com",
                PublicationId = Guid.NewGuid(),
                Role = Owner,
                CreatedById = createdById,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(existingPublicationInvite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new UserPublicationInviteRepository(contentDbContext);
                await repository.CreateManyIfNotExists(
                    userPublicationRoles: ListOf(
                        userPublicationRole1,
                        userPublicationRole2,
                        new UserPublicationRoleCreateRequest
                        {
                            PublicationId = existingPublicationInvite.PublicationId,
                            PublicationRole = existingPublicationInvite.Role,
                        }),
                    email: "test@test.com",
                    createdById: createdById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationInvites = await contentDbContext.UserPublicationInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Equal(3, userPublicationInvites.Count);

                Assert.Equal(existingPublicationInvite.Id, userPublicationInvites[0].Id);
                Assert.Equal(existingPublicationInvite.PublicationId, userPublicationInvites[0].PublicationId);
                Assert.Equal(existingPublicationInvite.Role, userPublicationInvites[0].Role);
                Assert.Equal("test@test.com", userPublicationInvites[0].Email);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationInvites[0].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userPublicationInvites[0].CreatedById);

                Assert.Equal(userPublicationRole1.PublicationId, userPublicationInvites[1].PublicationId);
                Assert.Equal(userPublicationRole1.PublicationRole, userPublicationInvites[1].Role);
                Assert.Equal("test@test.com", userPublicationInvites[1].Email);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationInvites[1].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userPublicationInvites[1].CreatedById);

                Assert.Equal(userPublicationRole2.PublicationId, userPublicationInvites[2].PublicationId);
                Assert.Equal(userPublicationRole2.PublicationRole, userPublicationInvites[2].Role);
                Assert.Equal("test@test.com", userPublicationInvites[2].Email);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationInvites[2].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userPublicationInvites[2].CreatedById);
            }
        }
    }
}
