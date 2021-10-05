#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task FindByEmail()
        {
            var user = new User
            {
                Email = "test@test.com"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserReleaseRoleRepository(contentDbContext);
                var result = await repository.FindByEmail("test@test.com");
                Assert.NotNull(result);
                Assert.Equal(user.Id, result!.Id);
            }
        }

        [Fact]
        public async Task FindByEmail_DifferentCase()
        {
            var user = new User
            {
                Email = "test@test.com"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserReleaseRoleRepository(contentDbContext);
                var result = await repository.FindByEmail("TEST@TEST.COM");

                // While this test could fail using the in-memory db if the comparison was accidentally case sensitive,
                // it wouldn't fail using SqlServer which is using a case insensitive collation by default.
                // C# equality is translated directly to SQL equality, which won't be case-sensitive with our config.
                // EF 5.0 will allow setting a different collation making this test worth while.
                // See https://docs.microsoft.com/en-us/ef/core/miscellaneous/collations-and-case-sensitivity
                Assert.NotNull(result);
                Assert.Equal(user.Id, result!.Id);
            }
        }

        [Fact]
        public async Task FindByEmail_NotFound()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = SetupUserReleaseRoleRepository(contentDbContext);
            var result = await repository.FindByEmail("test@test.com");
            Assert.Null(result);
        }

        private static UserRepository SetupUserReleaseRoleRepository(
            ContentDbContext contentDbContext)
        {
            return new(contentDbContext);
        }
    }
}
