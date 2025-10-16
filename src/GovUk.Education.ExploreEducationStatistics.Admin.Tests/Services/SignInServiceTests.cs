#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class SignInServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private static readonly Guid CreatedById = Guid.NewGuid();

    [Fact]
    public async Task RegisterOrSignIn_InvitedUser_ActivatesNewUser()
    {
        var firstName = "new";
        var lastName = "user";

        var invitedUser = _dataFixture
            .DefaultUserWithPendingInvite()
            .WithRole(
                new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE",
                }
            )
            .Generate();

        var userReleaseInvites = _dataFixture
            .DefaultUserReleaseInvite()
            .WithEmail(invitedUser.Email)
            .WithReleaseVersion(_dataFixture.DefaultReleaseVersion())
            .WithCreatedById(CreatedById)
            .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
            .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
            .ForIndex(2, s => s.SetRole(ReleaseRole.PrereleaseViewer))
            .GenerateList(3);

        var userPublicationInvites = _dataFixture
            .DefaultUserPublicationInvite()
            .WithEmail(invitedUser.Email)
            .WithPublication(_dataFixture.DefaultPublication())
            .WithCreatedById(CreatedById)
            .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
            .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
            .GenerateList(2);

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.Add(invitedUser);
            await contentDbContext.SaveChangesAsync();
        }

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.GetProfileFromClaims())
            .Returns(new UserProfileFromClaims(Email: invitedUser.Email, FirstName: firstName, LastName: lastName));

        var userManager = MockUserManager();
        userManager.Setup(mock => mock.FindByEmailAsync(invitedUser.Email)).ReturnsAsync((ApplicationUser?)null);
        userManager
            .Setup(mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), invitedUser.Role.Name))
            .ReturnsAsync(IdentityResult.Success);

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.GetInvitesByEmail(invitedUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userReleaseInvites);
        userReleaseInviteRepository
            .Setup(mock => mock.RemoveByUserEmail(invitedUser.Email, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
        userPublicationInviteRepository
            .Setup(mock => mock.GetInvitesByEmail(invitedUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPublicationInvites);
        userPublicationInviteRepository
            .Setup(mock => mock.RemoveByUserEmail(invitedUser.Email, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
        foreach (var userReleaseInvite in userReleaseInvites)
        {
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.Create(
                        It.IsAny<Guid>(),
                        userReleaseInvite.ReleaseVersionId,
                        userReleaseInvite.Role,
                        CreatedById
                    )
                )
                .ReturnsAsync(new UserReleaseRole());
        }

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
        foreach (var userPublicationInvite in userPublicationInvites)
        {
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.Create(
                        It.IsAny<Guid>(),
                        userPublicationInvite.PublicationId,
                        userPublicationInvite.Role,
                        CreatedById
                    )
                )
                .ReturnsAsync(new UserPublicationRole());
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userService: userService.Object,
                userManager: userManager.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object,
                userPublicationInviteRepository: userPublicationInviteRepository.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object
            );

            var result = await service.RegisterOrSignIn();

            var signInResponse = result.AssertRight();

            Assert.Equal(LoginResult.RegistrationSuccess, signInResponse.LoginResult);
            Assert.Equal(invitedUser.Id, signInResponse.UserProfile!.Id);
            Assert.Equal(firstName, signInResponse.UserProfile.FirstName);
        }

        VerifyAllMocks(
            userService,
            userManager,
            userReleaseInviteRepository,
            userPublicationInviteRepository,
            userReleaseRoleRepository,
            userPublicationRoleRepository
        );

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var newIdentityUser = await usersAndRolesDbContext.Users.SingleAsync(u => u.Email == invitedUser.Email);

            Assert.Equal(invitedUser.Id.ToString(), newIdentityUser.Id);
            Assert.Equal(invitedUser.Email, newIdentityUser.Email);
            Assert.Equal(invitedUser.Email, newIdentityUser.UserName);
            Assert.Equal(firstName, newIdentityUser.FirstName);
            Assert.Equal(lastName, newIdentityUser.LastName);

            var activatedUser = await contentDbContext.Users.SingleAsync(u => u.Email == invitedUser.Email);

            Assert.Equal(invitedUser.Email, activatedUser.Email);
            Assert.Equal(firstName, activatedUser.FirstName);
            Assert.Equal(lastName, activatedUser.LastName);
            Assert.True(activatedUser.Active);
            Assert.Null(activatedUser.SoftDeleted);
            Assert.Null(activatedUser.DeletedById);
            Assert.Equal(invitedUser.RoleId, activatedUser.RoleId);
            activatedUser.Created.AssertUtcNow();
            Assert.Equal(activatedUser.CreatedById, activatedUser.CreatedById);
        }
    }

    [Fact]
    public async Task RegisterOrSignIn_HasAspNetUser_SuccessfulLogin()
    {
        var aspNetUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@test.com",
            UserName = "test@test.com",
            FirstName = "existing",
            LastName = "user",
        };

        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            usersAndRolesDbContext.Users.Add(aspNetUser);
            await usersAndRolesDbContext.SaveChangesAsync();
        }

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.GetProfileFromClaims())
            .Returns(
                new UserProfileFromClaims(
                    Email: aspNetUser.Email,
                    FirstName: aspNetUser.FirstName,
                    LastName: aspNetUser.LastName
                )
            );

        var userManager = MockUserManager();
        userManager.Setup(mock => mock.FindByEmailAsync(aspNetUser.Email)).ReturnsAsync(aspNetUser);

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupService(
                usersAndRolesDbContext: usersAndRolesDbContext,
                userService: userService.Object,
                userManager: userManager.Object
            );

            var result = await service.RegisterOrSignIn();

            var signInResponse = result.AssertRight();

            Assert.Equal(LoginResult.LoginSuccess, signInResponse.LoginResult);
            Assert.Equal(Guid.Parse(aspNetUser.Id), signInResponse.UserProfile!.Id);
            Assert.Equal(aspNetUser.FirstName, signInResponse.UserProfile.FirstName);
        }

        VerifyAllMocks(userService, userManager);
    }

    [Fact]
    public async Task RegisterOrSignIn_SoftDeletedUser_ReturnsNoInviteResult()
    {
        var softDeletedUser = _dataFixture
            .DefaultSoftDeletedUser()
            .WithRole(
                new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE",
                }
            )
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.Add(softDeletedUser);
            await contentDbContext.SaveChangesAsync();
        }

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.GetProfileFromClaims())
            .Returns(
                new UserProfileFromClaims(
                    Email: softDeletedUser.Email,
                    FirstName: "testFirstName",
                    LastName: "testLastName"
                )
            );

        var userManager = MockUserManager();
        userManager.Setup(mock => mock.FindByEmailAsync(softDeletedUser.Email)).ReturnsAsync((ApplicationUser?)null);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                userService: userService.Object,
                userManager: userManager.Object
            );

            var result = await service.RegisterOrSignIn();

            var signInResponse = result.AssertRight();

            Assert.Equal(LoginResult.NoInvite, signInResponse.LoginResult);
            Assert.Null(signInResponse.UserProfile);
        }

        VerifyAllMocks(userService, userManager);
    }

    [Fact]
    public async Task RegisterOrSignIn_ExpiredInvite_RemovesAssociatedRoleInvites()
    {
        var firstName = "Bill";
        var lastName = "Piper";

        var userWithExpiredInvite = _dataFixture
            .DefaultUserWithExpiredInvite()
            .WithRole(
                new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE",
                }
            )
            .Generate();

        var userReleaseInvites = _dataFixture
            .DefaultUserReleaseInvite()
            .WithEmail(userWithExpiredInvite.Email)
            .WithReleaseVersion(_dataFixture.DefaultReleaseVersion())
            .WithCreatedById(CreatedById)
            .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
            .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
            .ForIndex(2, s => s.SetRole(ReleaseRole.PrereleaseViewer))
            .GenerateList(3);

        var userPublicationInvites = _dataFixture
            .DefaultUserPublicationInvite()
            .WithEmail(userWithExpiredInvite.Email)
            .WithPublication(_dataFixture.DefaultPublication())
            .WithCreatedById(CreatedById)
            .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
            .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
            .GenerateList(2);

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.Add(userWithExpiredInvite);
            await contentDbContext.SaveChangesAsync();
        }

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.GetProfileFromClaims())
            .Returns(
                new UserProfileFromClaims(Email: userWithExpiredInvite.Email, FirstName: firstName, LastName: lastName)
            );

        var userManager = MockUserManager();
        userManager
            .Setup(mock => mock.FindByEmailAsync(userWithExpiredInvite.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.RemoveByUserEmail(userWithExpiredInvite.Email, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
        userPublicationInviteRepository
            .Setup(mock => mock.RemoveByUserEmail(userWithExpiredInvite.Email, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userService: userService.Object,
                userManager: userManager.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object,
                userPublicationInviteRepository: userPublicationInviteRepository.Object
            );

            var result = await service.RegisterOrSignIn();

            var signInResponse = result.AssertRight();

            Assert.Equal(LoginResult.ExpiredInvite, signInResponse.LoginResult);
            Assert.Null(signInResponse.UserProfile);
        }

        VerifyAllMocks(userService, userManager, userReleaseInviteRepository, userPublicationInviteRepository);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var newIdentityUser = await usersAndRolesDbContext.Users.SingleOrDefaultAsync(u =>
                u.Email == userWithExpiredInvite.Email
            );

            Assert.Null(newIdentityUser);

            var user = await contentDbContext.Users.SingleAsync(u => u.Email == userWithExpiredInvite.Email);

            // Everything on the User entity should remain unchanged
            Assert.Equal(userWithExpiredInvite.Email, user.Email);
            Assert.Null(user.FirstName);
            Assert.Null(user.LastName);
            Assert.False(user.Active);
            Assert.Null(user.SoftDeleted);
            Assert.Null(user.DeletedById);
            Assert.Equal(userWithExpiredInvite.RoleId, user.RoleId);
            user.Created.AssertEqual(userWithExpiredInvite.Created);
            Assert.Equal(userWithExpiredInvite.CreatedById, user.CreatedById);
        }
    }

    private static SignInService SetupService(
        IUserService? userService = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        UserManager<ApplicationUser>? userManager = null,
        ContentDbContext? contentDbContext = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserReleaseInviteRepository? userReleaseInviteRepository = null,
        IUserPublicationInviteRepository? userPublicationInviteRepository = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new SignInService(
            logger: Mock.Of<ILogger<SignInService>>(),
            userService: userService ?? Mock.Of<IUserService>(),
            usersAndRolesDbContext: usersAndRolesDbContext,
            userManager: userManager ?? MockUserManager().Object,
            contentDbContext: contentDbContext,
            userReleaseRoleRepository: userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(),
            userPublicationRoleRepository: userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(),
            userReleaseInviteRepository: userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(),
            userPublicationInviteRepository: userPublicationInviteRepository
                ?? Mock.Of<IUserPublicationInviteRepository>()
        );
    }
}
