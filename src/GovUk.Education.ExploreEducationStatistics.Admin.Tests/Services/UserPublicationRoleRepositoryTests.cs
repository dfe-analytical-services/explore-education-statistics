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
            var userId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();
            
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserPublicationRoleRepository(contentDbContext);

                var result = await service.Create(userId, publicationId, Owner);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(userId, result.UserId);
                Assert.Equal(publicationId, result.PublicationId);
                Assert.Equal(Owner, result.Role);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();
                Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, userPublicationRoles[0].Id);
                Assert.Equal(userId, userPublicationRoles[0].UserId);
                Assert.Equal(publicationId, userPublicationRoles[0].PublicationId);
                Assert.Equal(Owner, userPublicationRoles[0].Role);
            }
        }

        [Fact]
        public async Task GetByRole()
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
                await contentDbContext.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserPublicationRoleRepository(contentDbContext);

                var result = await service.GetByRole(userPublicationRole.UserId, userPublicationRole.PublicationId, Owner);
                
                Assert.Equal(userPublicationRole.Id, result.Id);
                Assert.Equal(userPublicationRole.UserId, result.UserId);
                Assert.Equal(userPublicationRole.PublicationId, result.PublicationId);
                Assert.Equal(Owner, result.Role);
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
