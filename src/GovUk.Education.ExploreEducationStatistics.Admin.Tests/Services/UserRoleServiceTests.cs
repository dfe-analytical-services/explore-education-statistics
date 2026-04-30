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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPublicationRoleRepository;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserRoleServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private readonly User _user = new DataFixture().DefaultUser().WithId(Guid.NewGuid());

    public static readonly TheoryData<Func<DataFixture, User>> AllTypesOfUser =
    [
        // ActiveUser
        fixture => fixture.DefaultUser(),
        // User with Pending Invite
        fixture => fixture.DefaultUserWithPendingInvite(),
        // User with Expired Invite
        fixture => fixture.DefaultUserWithExpiredInvite(),
        // Soft Deleted User
        fixture => fixture.DefaultSoftDeletedUser(),
    ];

    public static readonly TheoryData<Func<DataFixture, User>> AllTypesOfNonActiveUser =
    [
        // User with Pending Invite
        fixture => fixture.DefaultUserWithPendingInvite(),
        // User with Expired Invite
        fixture => fixture.DefaultUserWithExpiredInvite(),
        // Soft Deleted User
        fixture => fixture.DefaultSoftDeletedUser(),
    ];

    public class SetGlobalRoleForUserTests : UserRoleServiceTests
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

    public class AddPublicationRoleTests : UserRoleServiceTests
    {
        [Fact]
        public async Task AddPublicationRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            UserPublicationRole createdUserPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication());

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.Analyst));

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasRoleOnPublication(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
            userPublicationRoleRepository
                .Setup(s =>
                    s.Create(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        _user.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserPublicationRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPublicationRole(createdUserPublicationRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Drafter);

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userManager, userPublicationRoleRepository);
        }

        [Fact]
        public async Task AddPublicationRole_HasNoRoles_UpgradesRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            UserPublicationRole createdUserPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication());

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasRoleOnPublication(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
            userPublicationRoleRepository
                .Setup(s =>
                    s.Create(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        _user.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserPublicationRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPublicationRole(createdUserPublicationRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync([]);

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Drafter);

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userManager);
        }

        [Fact]
        public async Task AddPublicationRole_HasHigherGlobalRoleThanAnalyst_DoesNotUpgradeRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            UserPublicationRole createdUserPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication());

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasRoleOnPublication(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
            userPublicationRoleRepository
                .Setup(s =>
                    s.Create(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        _user.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserPublicationRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPublicationRole(createdUserPublicationRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            var userManager = MockUserManager();

            // Here we are testing that if the user already has a higher-powered role than Analyst, there's no need
            // to assign them the lower-powered Analyst role.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.BauUser));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userManager: userManager.Object
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Drafter);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userResourceRoleNotificationService);

            userManager.Verify(
                mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
                Times.Never
            );

            userManager.Verify(
                mock => mock.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<List<string>>()),
                Times.Never
            );
        }

        [Fact]
        public async Task AddPublicationRole_HasLowerGlobalRoleThanAnalyst_UpgradesRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            UserPublicationRole createdUserPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication());

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasRoleOnPublication(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
            userPublicationRoleRepository
                .Setup(s =>
                    s.Create(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        _user.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserPublicationRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPublicationRole(createdUserPublicationRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            var userManager = MockUserManager();

            // Here we are checking to see that the user is "upgraded" to the higher-powered Analyst role by their
            // existing PreReleaseUser role being removed and the higher-powered Analyst role being added.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.PrereleaseUser));

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(user), ItIs.ListSequenceEqualTo(ListOf(RoleNames.PrereleaseUser)))
                )
                .ReturnsAsync(new IdentityResult());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Drafter);

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userManager);
        }

        [Fact]
        public async Task AddPublicationRole_UserAlreadyHasRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasRoleOnPublication(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Drafter);

                result.AssertBadRequest(UserAlreadyHasResourceRole);
            }

            VerifyAllMocks(userPublicationRoleRepository);

            userPublicationRoleRepository.Verify(
                s =>
                    s.Create(
                        It.IsAny<Guid>(),
                        It.IsAny<Guid>(),
                        It.IsAny<PublicationRole>(),
                        It.IsAny<Guid>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task AddPublicationRole_NoUser()
        {
            var publication = new Publication();

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext
                );

                var result = await service.AddPublicationRole(Guid.NewGuid(), publication.Id, PublicationRole.Drafter);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                Assert.Empty(userPublicationRoles);
            }
        }

        [Fact]
        public async Task AddPublicationRole_NoPublication()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser { Id = userId.ToString() };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext
                );

                var result = await service.AddPublicationRole(userId, Guid.NewGuid(), PublicationRole.Drafter);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                Assert.Empty(userPublicationRoles);
            }
        }
    }

    public class InviteDrafterTests : UserRoleServiceTests
    {
        [Fact]
        public async Task ActiveUserExists()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            Publication publication = _dataFixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            UserPublicationRole createdUserDrafterRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter);

            var userManager = MockUserManager();
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.Analyst));

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(_user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_user);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(_user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_user);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _user.Id,
                        publication.Id,
                        ResourceRoleFilter.All,
                        It.IsAny<CancellationToken>(),
                        new PublicationRole[] { PublicationRole.Drafter, PublicationRole.Approver }
                    )
                )
                .ReturnsAsync(false);
            userPublicationRoleRepository
                .Setup(s =>
                    s.Create(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        _user.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserDrafterRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewDrafterRole(createdUserDrafterRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteDrafter(_user.Email, publication.Id);

                result.AssertRight();
            }

            VerifyAllMocks(
                userResourceRoleNotificationService,
                userManager,
                userPublicationRoleRepository,
                userRepository
            );
        }

        [Fact]
        public async Task ActiveUserExists_HasNoRoles_UpgradesRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            Publication publication = _dataFixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            UserPublicationRole createdUserDrafterRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter);

            var userManager = MockUserManager();
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync([]);
            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(_user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_user);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(_user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_user);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _user.Id,
                        publication.Id,
                        ResourceRoleFilter.All,
                        It.IsAny<CancellationToken>(),
                        new PublicationRole[] { PublicationRole.Drafter, PublicationRole.Approver }
                    )
                )
                .ReturnsAsync(false);
            userPublicationRoleRepository
                .Setup(s =>
                    s.Create(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        _user.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserDrafterRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewDrafterRole(createdUserDrafterRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteDrafter(_user.Email, publication.Id);

                result.AssertRight();
            }

            VerifyAllMocks(
                userResourceRoleNotificationService,
                userManager,
                userPublicationRoleRepository,
                userRepository
            );
        }

        [Fact]
        public async Task ActiveUserExists_HasHigherGlobalRoleThanAnalyst_DoesNotUpgradeRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            Publication publication = _dataFixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            UserPublicationRole createdUserDrafterRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter);

            var userManager = MockUserManager();
            // Here we are testing that if the user already has a higher-powered role than Analyst, there's no need
            // to assign them the lower-powered Analyst role.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.BauUser));

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(_user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_user);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(_user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_user);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _user.Id,
                        publication.Id,
                        ResourceRoleFilter.All,
                        It.IsAny<CancellationToken>(),
                        new PublicationRole[] { PublicationRole.Drafter, PublicationRole.Approver }
                    )
                )
                .ReturnsAsync(false);
            userPublicationRoleRepository
                .Setup(s =>
                    s.Create(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        _user.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserDrafterRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewDrafterRole(createdUserDrafterRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteDrafter(_user.Email, publication.Id);

                result.AssertRight();
            }

            VerifyAllMocks(
                userResourceRoleNotificationService,
                userManager,
                userPublicationRoleRepository,
                userRepository
            );

            userManager.Verify(
                mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
                Times.Never
            );

            userManager.Verify(
                mock => mock.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<List<string>>()),
                Times.Never
            );
        }

        [Fact]
        public async Task ActiveUserExists_HasLowerGlobalRoleThanAnalyst_UpgradesRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            Publication publication = _dataFixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            UserPublicationRole createdUserDrafterRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter);

            var userManager = MockUserManager();
            // Here we are checking to see that the user is "upgraded" to the higher-powered Analyst role by their
            // existing PreReleaseUser role being removed and the higher-powered Analyst role being added.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.PrereleaseUser));
            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());
            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(user), ItIs.ListSequenceEqualTo(ListOf(RoleNames.PrereleaseUser)))
                )
                .ReturnsAsync(new IdentityResult());

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(_user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_user);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(_user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_user);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _user.Id,
                        publication.Id,
                        ResourceRoleFilter.All,
                        It.IsAny<CancellationToken>(),
                        new PublicationRole[] { PublicationRole.Drafter, PublicationRole.Approver }
                    )
                )
                .ReturnsAsync(false);
            userPublicationRoleRepository
                .Setup(s =>
                    s.Create(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        _user.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserDrafterRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewDrafterRole(createdUserDrafterRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteDrafter(_user.Email, publication.Id);

                result.AssertRight();
            }

            VerifyAllMocks(
                userResourceRoleNotificationService,
                userManager,
                userPublicationRoleRepository,
                userRepository
            );
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task UserAlreadyHasRole(Func<DataFixture, User> userFactory)
        {
            User existingUser = userFactory(_dataFixture);
            Publication publication = _dataFixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            UserPublicationRole createdUserDrafterRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(existingUser)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(existingUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        existingUser.Id,
                        publication.Id,
                        ResourceRoleFilter.All,
                        It.IsAny<CancellationToken>(),
                        new PublicationRole[] { PublicationRole.Drafter, PublicationRole.Approver }
                    )
                )
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteDrafter(_user.Email, publication.Id);

                result.AssertBadRequest(UserAlreadyHasResourceRoleOrMorePowerfulRole);
            }

            VerifyAllMocks(userPublicationRoleRepository, userRepository);
        }

        [Fact]
        public async Task UserDoesNotExist_CreatesUserAndAddsRole()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            User createdUser = _dataFixture.DefaultUserWithPendingInvite().WithId(_user.Id);

            UserPublicationRole createdUserDrafterRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(createdUser)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(createdUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(createdUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(createdUser.Email, Role.Analyst, _user.Id, null, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(createdUser);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(s =>
                    s.Create(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        _user.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserDrafterRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewDrafterRole(createdUserDrafterRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteDrafter(_user.Email, publication.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userPublicationRoleRepository, userRepository);
        }

        [Theory]
        [MemberData(nameof(AllTypesOfNonActiveUser))]
        public async Task UserDoesExistButNotActive_UpdatesUserAndAddsRole(Func<DataFixture, User> userFactory)
        {
            User existingUser = userFactory(_dataFixture);
            Publication publication = _dataFixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            UserPublicationRole createdUserDrafterRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(existingUser)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(existingUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(existingUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(existingUser.Email, Role.Analyst, _user.Id, null, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(existingUser);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        existingUser.Id,
                        publication.Id,
                        ResourceRoleFilter.All,
                        It.IsAny<CancellationToken>(),
                        new PublicationRole[] { PublicationRole.Drafter, PublicationRole.Approver }
                    )
                )
                .ReturnsAsync(false);
            userPublicationRoleRepository
                .Setup(s =>
                    s.Create(
                        existingUser.Id,
                        publication.Id,
                        PublicationRole.Drafter,
                        _user.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserDrafterRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewDrafterRole(createdUserDrafterRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteDrafter(existingUser.Email, publication.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userPublicationRoleRepository, userRepository);
        }

        [Fact]
        public async Task PublicationDoesNotExist_ReturnsNotFound()
        {
            var service = SetupService();

            var result = await service.InviteDrafter("test@test.com", Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    public class UpdatePublicationDraftersTests : UserRoleServiceTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                // This one should be removed as we are not supplying the userId for it
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(_dataFixture.DefaultUser())
                            .SetPublication(publication)
                            .SetRole(PublicationRole.Drafter)
                )
                // This one should remain as we are supplying the userId for it
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(_dataFixture.DefaultUser())
                            .SetPublication(publication)
                            .SetRole(PublicationRole.Drafter)
                )
                // This one should remain as it's an existing Approver role, and another Drafter role should be created for this user
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_dataFixture.DefaultUser())
                            .SetPublication(publication)
                            .SetRole(PublicationRole.Approver)
                )
                // This one should remain as it's for a different publication, and another Drafter role should be created for this user
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(_dataFixture.DefaultUser())
                            .SetPublication(_dataFixture.DefaultPublication())
                            .SetRole(PublicationRole.Drafter)
                )
                .GenerateList(4);

            var newUserId = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, [.. userPublicationRoles]);
            // The first role should be removed
            userPublicationRoleRepository
                .Setup(m => m.RemoveMany(ListOf(userPublicationRoles[0]), default))
                .Returns(Task.CompletedTask);
            // Three roles should be created
            userPublicationRoleRepository
                .Setup(m =>
                    m.CreateManyIfNotExists(
                        It.Is<List<UserPublicationRoleCreateDto>>(l =>
                            l.Count == 3
                            && l.All(upr =>
                                upr.PublicationId == publication.Id
                                && upr.Role == PublicationRole.Drafter
                                && Math.Abs((upr.CreatedDate!.Value - DateTime.UtcNow).Milliseconds)
                                    <= AssertExtensions.TimeWithinMillis
                                && upr.CreatedById == _user.Id
                            )
                            && l[0].UserId == newUserId
                            && l[1].UserId == userPublicationRoles[2].UserId
                            && l[2].UserId == userPublicationRoles[3].UserId
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync([]); // Don't actually need to return anything here for the test. Just want to check it was called correctly.

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.UpdatePublicationDrafters(
                    publicationId: publication.Id,
                    userIds:
                    [
                        newUserId,
                        userPublicationRoles[1].UserId,
                        userPublicationRoles[2].UserId,
                        userPublicationRoles[3].UserId,
                    ]
                );
                result.AssertRight();
            }

            VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task PublicationDoesNotExist_ReturnsNotFound()
        {
            var service = SetupService();

            var result = await service.UpdatePublicationDrafters(publicationId: Guid.NewGuid(), userIds: []);

            result.AssertNotFound();
        }
    }

    public class UpgradeToGlobalRoleIfRequiredTests : UserRoleServiceTests
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

                await service.UpgradeToGlobalRoleIfRequired(RoleNames.Analyst, user);

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

                await service.UpgradeToGlobalRoleIfRequired(RoleNames.Analyst, user);

                VerifyAllMocks(userManager);
            }
        }
    }

    public class GetAllGlobalRolesTests : UserRoleServiceTests
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

    public class GetGlobalRolesForUserTests : UserRoleServiceTests
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

    public class GetPublicationRolesForUserTests : UserRoleServiceTests
    {
        [Fact]
        public async Task GetPublicationRolesForUser()
        {
            User user = _dataFixture.DefaultUser();

            UserPublicationRole userPublicationRole1 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication());

            UserPublicationRole userPublicationRole2 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication());

            // Role assignment for a different user
            UserPublicationRole userPublicationRole3 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication());

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository.Setup(m => m.FindActiveUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(
                ResourceRoleFilter.ActiveOnly,
                [userPublicationRole1, userPublicationRole2, userPublicationRole3]
            );

            var service = SetupService(
                userRepository: userRepository.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object
            );

            var result = await service.GetPublicationRolesForUser(user.Id);

            result.AssertRight();

            var userPublicationRoles = result.Right;
            Assert.Equal(2, userPublicationRoles.Count);

            Assert.Equal(userPublicationRole1.Id, userPublicationRoles[0].Id);
            Assert.Equal(userPublicationRole1.Publication.Title, userPublicationRoles[0].Publication);
            Assert.Equal(userPublicationRole1.Role, userPublicationRoles[0].Role);

            Assert.Equal(userPublicationRole2.Id, userPublicationRoles[1].Id);
            Assert.Equal(userPublicationRole2.Publication.Title, userPublicationRoles[1].Publication);
            Assert.Equal(userPublicationRole2.Role, userPublicationRoles[1].Role);

            VerifyAllMocks(userRepository, userPublicationRoleRepository);
        }

        [Fact]
        public async Task GetPublicationRolesForUser_NoUser()
        {
            var userId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(m => m.FindActiveUserById(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = SetupService(userRepository: userRepository.Object);

            var result = await service.GetPublicationRolesForUser(userId);

            result.AssertNotFound();

            VerifyAllMocks(userRepository);
        }
    }

    public class GetPublicationRolesForPublicationTests : UserRoleServiceTests
    {
        [Fact]
        public async Task GetPublicationRolesForPublication()
        {
            User user1 = _dataFixture.DefaultUser();

            User user2 = _dataFixture.DefaultUser();

            Publication publication = _dataFixture.DefaultPublication();

            UserPublicationRole userPublicationRole1 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user2)
                .WithPublication(publication);

            UserPublicationRole userPublicationRole2 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication);

            // Role assignment for a different publication
            UserPublicationRole userPublicationRole3 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(_dataFixture.DefaultPublication());

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(
                ResourceRoleFilter.ActiveOnly,
                [userPublicationRole1, userPublicationRole2, userPublicationRole3]
            );

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.GetPublicationRolesForPublication(publication.Id);

                result.AssertRight();

                var userPublicationRoles = result.Right;
                Assert.Equal(2, userPublicationRoles.Count);

                Assert.Equal(userPublicationRole2.Id, userPublicationRoles[0].Id);
                Assert.Equal(userPublicationRole2.Publication.Title, userPublicationRoles[0].Publication);
                Assert.Equal(user1.DisplayName, userPublicationRoles[0].UserName);
                Assert.Equal(userPublicationRole2.Role, userPublicationRoles[0].Role);
                Assert.Equal(user1.Email, userPublicationRoles[0].Email);

                Assert.Equal(userPublicationRole1.Id, userPublicationRoles[1].Id);
                Assert.Equal(userPublicationRole1.Publication.Title, userPublicationRoles[1].Publication);
                Assert.Equal(user2.DisplayName, userPublicationRoles[1].UserName);
                Assert.Equal(userPublicationRole1.Role, userPublicationRoles[1].Role);
                Assert.Equal(user2.Email, userPublicationRoles[1].Email);

                VerifyAllMocks(userPublicationRoleRepository);
            }
        }

        [Fact]
        public async Task GetPublicationRolesForPublication_NoPublication()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupService(contentDbContext: contentDbContext);

            var result = await service.GetPublicationRolesForPublication(Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    public class RemoveUserPublicationRoleTests : UserRoleServiceTests
    {
        [Fact]
        public async Task RemoveUserPublicationRole()
        {
            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));
            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst)))
                )
                .ReturnsAsync(new IdentityResult());

            userPublicationRoleRepository
                .Setup(m => m.GetById(userPublicationRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPublicationRole);
            userPublicationRoleRepository.Setup(m => m.RemoveById(userPublicationRole.Id, default)).ReturnsAsync(true);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository);
        }

        [Fact]
        public async Task RemoveUserPublicationRole_NoUserPublicationRole()
        {
            var userPublicationRoleGuid = Guid.NewGuid();

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(m => m.GetById(userPublicationRoleGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserPublicationRole?)null);

            var service = SetupService(userPublicationRoleRepository: userPublicationRoleRepository.Object);

            var result = await service.RemoveUserPublicationRole(userPublicationRoleGuid);

            result.AssertNotFound();

            VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task RemoveUserPublicationRole_RoleRemovalFails()
        {
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(m => m.GetById(userPublicationRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPublicationRole);
            userPublicationRoleRepository.Setup(m => m.RemoveById(userPublicationRole.Id, default)).ReturnsAsync(false);

            var service = SetupService(userPublicationRoleRepository: userPublicationRoleRepository.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RemoveUserPublicationRole(userPublicationRole.Id)
            );

            VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task RemoveUserPublicationRole_HasHigherGlobalRoleThanAnalyst()
        {
            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the BAU role, which is higher-powered than Analyst, so we will test that they remain
            // a BAU user and do not have any roles removed.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.BauUser));

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(m => m.GetById(userPublicationRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPublicationRole);
            userPublicationRoleRepository.Setup(m => m.RemoveById(userPublicationRole.Id, default)).ReturnsAsync(true);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task RemoveUserPublicationRole_AnalystRoleStillRequiredForOtherPublications()
        {
            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            UserPublicationRole anotherUserPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they retain the Analyst role  because
            // they still have need of it in other Publication roles.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(m => m.GetById(userPublicationRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPublicationRole);
            userPublicationRoleRepository.Setup(m => m.RemoveById(userPublicationRole.Id, default)).ReturnsAsync(true);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, anotherUserPublicationRole);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task RemoveUserPublicationRole_DowngradeToPreReleaseRole()
        {
            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            UserReleaseRole preReleaseRole = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                );

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they lose the Analyst role but gain the
            // PreReleaseUser role, as they have need of this role elsewhere.
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
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, preReleaseRole);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(m => m.GetById(userPublicationRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPublicationRole);
            userPublicationRoleRepository.Setup(m => m.RemoveById(userPublicationRole.Id, default)).ReturnsAsync(true);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, []);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userPreReleaseRoleRepository);
        }
    }

    public class RemoveAllUserResourceRolesTests : UserRoleServiceTests
    {
        [Fact]
        public async Task RemoveAllUserResourceRoles()
        {
            User targetUser = _dataFixture.DefaultUser();
            var targetIdentityUser = new ApplicationUser { Id = targetUser.Id.ToString() };

            User otherUser = _dataFixture.DefaultUser();
            var otherIdentityUser = new ApplicationUser { Id = otherUser.Id.ToString() };

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.AddRange(targetIdentityUser, otherIdentityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(targetIdentityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));
            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(
                        ItIsUser(targetIdentityUser),
                        ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst))
                    )
                )
                .ReturnsAsync(new IdentityResult());

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(m => m.FindActiveUserById(targetUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(targetUser);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository
                .Setup(m => m.RemoveForUser(targetUser.Id, default))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(m => m.RemoveForUser(targetUser.Id, default))
                .Returns(Task.CompletedTask);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.RemoveAllUserResourceRoles(targetUser.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userRepository, userPreReleaseRoleRepository, userPublicationRoleRepository);
        }
    }

    private static ApplicationUser ItIsUser(ApplicationUser user)
    {
        return It.Is<ApplicationUser>(applicationUser => applicationUser.Id == user.Id);
    }

    private UserRoleService SetupService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserPreReleaseRoleRepository? userPreReleaseRoleRepository = null,
        IUserRepository? userRepository = null,
        UserManager<ApplicationUser>? userManager = null,
        IUserService? userService = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        contentDbContext.Users.Add(_user);
        contentDbContext.SaveChanges();

        return new UserRoleService(
            usersAndRolesDbContext,
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
            userResourceRoleNotificationService ?? new Mock<IUserResourceRoleNotificationService>(Strict).Object,
            userService ?? AlwaysTrueUserService(_user.Id).Object,
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            userPreReleaseRoleRepository ?? Mock.Of<IUserPreReleaseRoleRepository>(Strict),
            userRepository ?? Mock.Of<IUserRepository>(Strict),
            userManager ?? MockUserManager().Object
        );
    }
}
