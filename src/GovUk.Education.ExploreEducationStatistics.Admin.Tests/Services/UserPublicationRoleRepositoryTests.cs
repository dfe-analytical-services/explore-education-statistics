using System;
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
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
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
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationRoles[0].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdBy.Id, userPublicationRoles[0].CreatedById);
            }
        }

        [Fact]
        public async Task GetByUserAndRole()
        {
            var userPublicationRole = new UserPublicationRole
            {
                User = new User(),
                Publication = new Publication(),
                Role = Owner,
                Created = DateTime.UtcNow,
                CreatedBy = new User()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserPublicationRoleRepository(contentDbContext);

                var result = await service.GetByUserAndRole(userPublicationRole.UserId, userPublicationRole.PublicationId,
                    Owner);

                Assert.Equal(userPublicationRole.Id, result.Id);
                Assert.Equal(userPublicationRole.UserId, result.UserId);
                Assert.Equal(userPublicationRole.PublicationId, result.PublicationId);
                Assert.Equal(Owner, result.Role);
                Assert.Equal(userPublicationRole.Created, result.Created);
                Assert.Equal(userPublicationRole.CreatedById, result.CreatedById);
            }
        }

        private static UserPublicationRoleRepository SetupUserPublicationRoleRepository(
            ContentDbContext contentDbContext)
        {
            return new UserPublicationRoleRepository(
                contentDbContext
            );
        }
    }
}
