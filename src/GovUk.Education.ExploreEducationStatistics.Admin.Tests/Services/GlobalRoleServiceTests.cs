#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class GlobalRoleServiceTests
{
    private readonly User _user = new DataFixture().DefaultUser().WithId(Guid.NewGuid());

    public class SetGlobalRoleForUserTests : GlobalRoleServiceTests
    {
        [Theory]
        [InlineData(Role.StandardUser, Role.StandardUser, Role.StandardUser)]
        [InlineData(Role.StandardUser, Role.BauUser, Role.BauUser)]
        [InlineData(Role.BauUser, Role.StandardUser, Role.StandardUser)]
        [InlineData(Role.BauUser, Role.BauUser, Role.BauUser)]
        [InlineData(null, Role.StandardUser, Role.StandardUser)]
        [InlineData(null, Role.BauUser, Role.BauUser)]
        public async Task Success(Role? oldRole, Role newRole, Role expectedUpdatedRole)
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId.ToString() };

            var hasOldRole = oldRole is not null;

            var existingIdentityRole = hasOldRole
                ? new IdentityRole { Id = oldRole!.GetEnumValue(), Name = oldRole!.GetEnumLabel() }
                : null;

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(user);

                if (hasOldRole)
                {
                    userAndRolesDbContext.Roles.Add(existingIdentityRole!);
                }

                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager
                .Setup(mock => mock.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync(hasOldRole ? [oldRole!.GetEnumLabel()] : []);

            userManager
                .Setup(mock => mock.AddToRoleAsync(ItIsUser(user), expectedUpdatedRole.GetEnumLabel()))
                .ReturnsAsync(new IdentityResult());

            if (hasOldRole)
            {
                userManager
                    .Setup(mock => mock.RemoveFromRoleAsync(ItIsUser(user), oldRole!.GetEnumLabel()))
                    .ReturnsAsync(new IdentityResult());
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    identityUserManager: userManager.Object
                );

                var result = await service.UpdateGlobalRoleForUser(userId, newRole);

                result.AssertRight();
            }

            VerifyAllMocks(userManager);
        }

        [Fact]
        public async Task NoUser_ReturnsNotFound()
        {
            var service = SetupService();

            var result = await service.UpdateGlobalRoleForUser(Guid.NewGuid(), Role.StandardUser);

            result.AssertNotFound();
        }

        [Fact]
        public async Task IdentityUserManagerReturnsMoreThanOneGlobalRole_Throws()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId.ToString() };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager
                .Setup(mock => mock.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync([Role.StandardUser.GetEnumLabel(), Role.BauUser.GetEnumLabel()]);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    identityUserManager: userManager.Object
                );

                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await service.UpdateGlobalRoleForUser(userId, Role.StandardUser)
                );
            }

            VerifyAllMocks(userManager);
        }
    }

    private static ApplicationUser ItIsUser(ApplicationUser user)
    {
        return It.Is<ApplicationUser>(applicationUser => applicationUser.Id == user.Id);
    }

    private GlobalRoleService SetupService(
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        UserManager<ApplicationUser>? identityUserManager = null,
        IUserService? userService = null
    )
    {
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new GlobalRoleService(
            usersAndRolesDbContext,
            userService ?? AlwaysTrueUserService(_user.Id).Object,
            identityUserManager ?? MockUserManager().Object
        );
    }
}
