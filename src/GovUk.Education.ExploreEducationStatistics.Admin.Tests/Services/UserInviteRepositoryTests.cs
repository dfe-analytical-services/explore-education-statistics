#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserInviteRepositoryTests
    {
        private static readonly Guid CreatedById = Guid.NewGuid();

        [Fact]
        public async Task CreateOrUpdate_RoleArgument()
        {
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var repository = new UserInviteRepository(usersAndRolesDbContext);
                var userInvite = await repository.CreateOrUpdate("test@test.com", Role.Analyst, CreatedById);

                Assert.Equal("test@test.com", userInvite.Email);
                Assert.Equal(Role.Analyst.GetEnumValue(), userInvite.RoleId);
                Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userInvite.Created).Milliseconds, 0, 1500);
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvite = usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .Single();

                Assert.Equal("test@test.com", userInvite.Email);
                Assert.Equal(Role.Analyst.GetEnumValue(), userInvite.RoleId);
                Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userInvite.Created).Milliseconds, 0, 1500);
            }
        }


        [Fact]
        public async Task CreateOrUpdate_RoleArgument_ExistingInvite()
        {
            var originalCreatedDate = DateTime.UtcNow.AddDays(-1);
            var newCreatedDate = DateTime.UtcNow;
            
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await usersAndRolesDbContext.AddAsync(new UserInvite
                {
                    Email = "test@test.com",
                    RoleId = Role.BauUser.GetEnumValue(),
                    CreatedById = CreatedById.ToString(),
                    Created = originalCreatedDate
                });
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var repository = new UserInviteRepository(usersAndRolesDbContext);
                var userInvite = await repository.CreateOrUpdate(
                    "test@test.com", 
                    Role.Analyst, 
                    CreatedById, 
                    createdDate: newCreatedDate);

                Assert.Equal("test@test.com", userInvite.Email);
                Assert.Equal(Role.Analyst.GetEnumValue(), userInvite.RoleId);
                Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
                Assert.Equal(newCreatedDate, userInvite.Created);
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvite = usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .Single();

                Assert.Equal("test@test.com", userInvite.Email);
                Assert.Equal(Role.Analyst.GetEnumValue(), userInvite.RoleId);
                Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
                Assert.Equal(newCreatedDate, userInvite.Created);
            }
        }

        [Fact]
        public async Task CreateOrUpdate_RoleIdStringArgument()
        {
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var repository = new UserInviteRepository(usersAndRolesDbContext);
                var userInvite =
                    await repository.CreateOrUpdate("test@test.com", Role.Analyst.GetEnumValue(), CreatedById);

                Assert.Equal("test@test.com", userInvite.Email);
                Assert.Equal(Role.Analyst.GetEnumValue(), userInvite.RoleId);
                Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userInvite.Created).Milliseconds, 0, 1500);
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvite = usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .Single();

                Assert.Equal("test@test.com", userInvite.Email);
                Assert.Equal(Role.Analyst.GetEnumValue(), userInvite.RoleId);
                Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userInvite.Created).Milliseconds, 0, 1500);
            }
        }
    }
}
