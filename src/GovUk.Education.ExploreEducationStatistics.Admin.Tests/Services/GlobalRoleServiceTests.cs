#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class GlobalRoleServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private readonly User _user = new DataFixture().DefaultUser().WithId(Guid.NewGuid());

    public class SetGlobalRoleForUserTests : GlobalRoleServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

            var role = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Role" };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(mock => mock.GetRolesAsync(ItIsUser(user))).ReturnsAsync(new List<string>());

            userManager
                .Setup(mock => mock.AddToRoleAsync(ItIsUser(user), role.Name))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                var result = await service.SetGlobalRoleForUser(user.Id, role.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();

                userManager.Verify(
                    mock =>
                        mock.AddToRoleAsync(
                            It.Is<ApplicationUser>(applicationUser => applicationUser.Id == user.Id),
                            role.Name
                        ),
                    Times.Once
                );
            }
        }

        [Fact]
        public async Task AlreadyHasSameRole()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

            var role = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Role" };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(mock => mock.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(role.Name));

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                var result = await service.SetGlobalRoleForUser(user.Id, role.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();

                VerifyAllMocks(userManager);
            }
        }

        [Fact]
        public async Task AlreadyHasAnotherRole()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

            var newRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "New Role" };

            var existingRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Existing Role" };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddRangeAsync(newRole, existingRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here we are setting up the user so that they have a different Global Role currently assigned
            // than the new one being set.  They will have the existing one removed and the new one added.
            userManager.Setup(mock => mock.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(existingRole.Name));

            userManager
                .Setup(mock => mock.AddToRoleAsync(ItIsUser(user), newRole.Name))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(mock =>
                    mock.RemoveFromRolesAsync(ItIsUser(user), ItIs.ListSequenceEqualTo(ListOf(existingRole.Name)))
                )
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                var result = await service.SetGlobalRoleForUser(user.Id, newRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();

                VerifyAllMocks(userManager);
            }
        }

        [Fact]
        public async Task NoRole_ReturnsNotFound()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                var result = await service.SetGlobalRoleForUser(user.Id, Guid.NewGuid().ToString());

                result.AssertNotFound();
            }

            userManager.VerifySet(mock => mock.Logger = null, Times.Once());
            VerifyAllMocks(userManager);
        }

        [Fact]
        public async Task NoUser_ReturnsNotFound()
        {
            var role = new IdentityRole { Id = Guid.NewGuid().ToString() };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                var result = await service.SetGlobalRoleForUser(Guid.NewGuid().ToString(), role.Id);

                result.AssertNotFound();
            }

            VerifyAllMocks(userManager);
        }
    }

    public class UpgradeToGlobalRoleIfRequiredTests : GlobalRoleServiceTests
    {
        [Fact]
        public async Task UpgradeToGlobalRoleIfRequired_DoUpgrade()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId.ToString() };

            var existingRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = RoleNames.PrereleaseUser };

            var newRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = RoleNames.Analyst };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddRangeAsync(user);
                await userAndRolesDbContext.Roles.AddRangeAsync(existingRole, newRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here we are setting up the user so that they have a different Global Role currently assigned
            // than the new one being set. They will have the existing one removed and the new one added.
            userManager.Setup(mock => mock.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(existingRole.Name));

            userManager
                .Setup(mock => mock.AddToRoleAsync(ItIsUser(user), newRole.Name))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(mock =>
                    mock.RemoveFromRolesAsync(ItIsUser(user), ItIs.ListSequenceEqualTo(ListOf(existingRole.Name)))
                )
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                await service.UpgradeToGlobalRoleIfRequired(user, RoleNames.Analyst);

                VerifyAllMocks(userManager);
            }
        }

        [Fact]
        public async Task UpgradeToGlobalRoleIfRequired_DoNotUpgrade()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId.ToString() };

            var existingRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = RoleNames.Analyst };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddRangeAsync(user);
                await userAndRolesDbContext.Roles.AddRangeAsync(existingRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // user already has the correct role
            userManager.Setup(mock => mock.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(existingRole.Name));

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                await service.UpgradeToGlobalRoleIfRequired(user, RoleNames.Analyst);

                VerifyAllMocks(userManager);
            }
        }
    }

    public class DowngradeFromGlobalRoleIfRequiredTests : GlobalRoleServiceTests
    {
        [Fact]
        public async Task TryDowngradeFromPrerelease_HasPrereleaseRoles_DoesNothing()
        {
            var userId = Guid.NewGuid();
            var identityUser = new ApplicationUser { Id = userId.ToString() };
            var user = _dataFixture.DefaultUser().WithId(userId);
            var userPreReleaseRole = _dataFixture.DefaultUserPreReleaseRole().WithUser(user);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.PrereleaseUser));

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, userPreReleaseRole);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                await service.DowngradeFromGlobalRoleIfRequired(identityUser, RoleNames.PrereleaseUser);
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task TryDowngradeFromPrerelease_HasPublicationRoles_UpgradesToAnalyst()
        {
            var userId = Guid.NewGuid();
            var identityUser = new ApplicationUser { Id = userId.ToString() };
            var user = _dataFixture.DefaultUser().WithId(userId);
            var userPublicationRole = _dataFixture.DefaultUserPublicationRole().WithUser(user);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.PrereleaseUser));
            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(
                        ItIsUser(identityUser),
                        ItIs.ListSequenceEqualTo(ListOf(RoleNames.PrereleaseUser))
                    )
                )
                .ReturnsAsync(new IdentityResult());
            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(identityUser), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, userPublicationRole);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                await service.DowngradeFromGlobalRoleIfRequired(identityUser, RoleNames.PrereleaseUser);
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }

        [Theory]
        [InlineData(RoleNames.Analyst)]
        [InlineData(RoleNames.BauUser)]
        public async Task TryDowngradeFromPrerelease_HasHigherGlobalRole_DoesNothing(string globalRole)
        {
            var userId = Guid.NewGuid();
            var identityUser = new ApplicationUser { Id = userId.ToString() };
            var user = _dataFixture.DefaultUser().WithId(userId);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Has higher global role assigned, so should not be downgraded
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(globalRole));

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                await service.DowngradeFromGlobalRoleIfRequired(identityUser, RoleNames.PrereleaseUser);
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task TryDowngradeFromPrerelease_HasNoHigherGlobalRoleOrPublicationRolesOrPreReleaseRoles_Downgrades()
        {
            var userId = Guid.NewGuid();
            var identityUser = new ApplicationUser { Id = userId.ToString() };
            var user = _dataFixture.DefaultUser().WithId(userId);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.PrereleaseUser));
            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(
                        ItIsUser(identityUser),
                        ItIs.ListSequenceEqualTo(ListOf(RoleNames.PrereleaseUser))
                    )
                )
                .ReturnsAsync(new IdentityResult());

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                await service.DowngradeFromGlobalRoleIfRequired(identityUser, RoleNames.PrereleaseUser);
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task TryDowngradeFromAnalyst_HasPrereleaseRoles_Downgrades()
        {
            var userId = Guid.NewGuid();
            var identityUser = new ApplicationUser { Id = userId.ToString() };
            var user = _dataFixture.DefaultUser().WithId(userId);
            var userPreReleaseRole = _dataFixture.DefaultUserPreReleaseRole().WithUser(user);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));
            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst)))
                )
                .ReturnsAsync(new IdentityResult());
            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(identityUser), RoleNames.PrereleaseUser))
                .ReturnsAsync(new IdentityResult());

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, userPreReleaseRole);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                await service.DowngradeFromGlobalRoleIfRequired(identityUser, RoleNames.Analyst);
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task TryDowngradeFromAnalyst_HasPublicationRoles_DoesNothing()
        {
            var userId = Guid.NewGuid();
            var identityUser = new ApplicationUser { Id = userId.ToString() };
            var user = _dataFixture.DefaultUser().WithId(userId);
            var userPublicationRole = _dataFixture.DefaultUserPublicationRole().WithUser(user);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, userPublicationRole);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                await service.DowngradeFromGlobalRoleIfRequired(identityUser, RoleNames.Analyst);
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task TryDowngradeFromAnalyst_HasHigherGlobalRole_DoesNothing()
        {
            var userId = Guid.NewGuid();
            var identityUser = new ApplicationUser { Id = userId.ToString() };
            var user = _dataFixture.DefaultUser().WithId(userId);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Has higher global role assigned, so should not be downgraded
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.BauUser));

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                await service.DowngradeFromGlobalRoleIfRequired(identityUser, RoleNames.Analyst);
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task TryDowngradeFromAnalyst_HasNoHigherGlobalRoleOrPublicationRolesOrPreReleaseRoles_Downgrades()
        {
            var userId = Guid.NewGuid();
            var identityUser = new ApplicationUser { Id = userId.ToString() };
            var user = _dataFixture.DefaultUser().WithId(userId);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));
            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst)))
                )
                .ReturnsAsync(new IdentityResult());

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                await service.DowngradeFromGlobalRoleIfRequired(identityUser, RoleNames.Analyst);
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }
    }

    public class GetAllGlobalRolesTests : GlobalRoleServiceTests
    {
        [Fact]
        public async Task GetAllGlobalRoles()
        {
            var role1 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1",
            };

            var role2 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 2",
                NormalizedName = "ROLE 2",
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddRangeAsync(role1, role2);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(usersAndRolesDbContext: userAndRolesDbContext);

                var result = await service.GetAllGlobalRoles();

                result.AssertRight();

                var globalRoles = result.Right.ToList();
                Assert.Equal(2, globalRoles.Count);

                Assert.Equal(role1.Id, globalRoles[0].Id);
                Assert.Equal(role1.Name, globalRoles[0].Name);
                Assert.Equal(role1.NormalizedName, globalRoles[0].NormalizedName);

                Assert.Equal(role2.Id, globalRoles[1].Id);
                Assert.Equal(role2.Name, globalRoles[1].Name);
                Assert.Equal(role2.NormalizedName, globalRoles[1].NormalizedName);
            }
        }
    }

    public class GetGlobalRolesForUserTests : GlobalRoleServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

            var role1 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1",
            };

            var role2 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 2",
                NormalizedName = "ROLE 2",
            };

            // Role not assigned
            var role3 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 3",
                NormalizedName = "ROLE 3",
            };

            var userRole1 = new IdentityUserRole<string> { UserId = user.Id, RoleId = role1.Id };

            var userRole2 = new IdentityUserRole<string> { UserId = user.Id, RoleId = role2.Id };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddRangeAsync(role1, role2, role3);
                await userAndRolesDbContext.AddRangeAsync(userRole1, userRole2);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(usersAndRolesDbContext: userAndRolesDbContext);

                var result = await service.GetGlobalRolesForUser(user.Id);

                result.AssertRight();

                var globalRoles = result.Right;
                Assert.Equal(2, globalRoles.Count);

                Assert.Equal(role1.Id, globalRoles[0].Id);
                Assert.Equal(role1.Name, globalRoles[0].Name);
                Assert.Equal(role1.NormalizedName, globalRoles[0].NormalizedName);

                Assert.Equal(role2.Id, globalRoles[1].Id);
                Assert.Equal(role2.Name, globalRoles[1].Name);
                Assert.Equal(role2.NormalizedName, globalRoles[1].NormalizedName);
            }
        }

        [Fact]
        public async Task NoUser_ReturnsNotFound()
        {
            await using var userAndRolesDbContext = InMemoryUserAndRolesDbContext();
            var service = SetupService(usersAndRolesDbContext: userAndRolesDbContext);

            var result = await service.GetGlobalRolesForUser(Guid.NewGuid().ToString());
            result.AssertNotFound();
        }
    }

    private static ApplicationUser ItIsUser(ApplicationUser user)
    {
        return It.Is<ApplicationUser>(applicationUser => applicationUser.Id == user.Id);
    }

    private GlobalRoleService SetupService(
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserPreReleaseRoleRepository? userPreReleaseRoleRepository = null,
        UserManager<ApplicationUser>? userManager = null,
        IUserService? userService = null
    )
    {
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new GlobalRoleService(
            usersAndRolesDbContext,
            usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
            userService ?? AlwaysTrueUserService(_user.Id).Object,
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            userPreReleaseRoleRepository ?? Mock.Of<IUserPreReleaseRoleRepository>(Strict),
            userManager ?? MockUserManager().Object
        );
    }
}
