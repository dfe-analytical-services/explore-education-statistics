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
        const string firstNameOld = "oldFirst";
        const string firstNameNew = "newFirst";
        const string lastNameOld = "oldLast";
        const string lastNameNew = "newLast";

        User invitedUser = _dataFixture
            .DefaultUserWithPendingInvite()
            .WithFirstName(firstNameOld)
            .WithLastName(lastNameOld)
            .WithRole(
                new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE",
                }
            );

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(invitedUser)
            .WithReleaseVersion(_dataFixture.DefaultReleaseVersion())
            .WithCreatedById(CreatedById)
            .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
            .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
            .ForIndex(2, s => s.SetRole(ReleaseRole.PrereleaseViewer))
            .GenerateList(3);

        var userPublicationRoles = _dataFixture
            .DefaultUserPublicationRole()
            .WithUser(invitedUser)
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
            .Returns(
                new UserProfileFromClaims(Email: invitedUser.Email, FirstName: firstNameNew, LastName: lastNameNew)
            );

        var userManager = MockUserManager();
        userManager.Setup(mock => mock.FindByEmailAsync(invitedUser.Email)).ReturnsAsync((ApplicationUser?)null);
        userManager
            .Setup(mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), invitedUser.Role!.Name!))
            .ReturnsAsync(IdentityResult.Success);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userService: userService.Object,
                userManager: userManager.Object
            );

            var result = await service.RegisterOrSignIn();

            var signInResponse = result.AssertRight();

            Assert.Equal(LoginResult.RegistrationSuccess, signInResponse.LoginResult);
            Assert.Equal(invitedUser.Id, signInResponse.UserProfile!.Id);
            Assert.Equal(firstNameNew, signInResponse.UserProfile.FirstName);
        }

        VerifyAllMocks(userService, userManager);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var newIdentityUser = await usersAndRolesDbContext.Users.SingleAsync(u => u.Email == invitedUser.Email);

            Assert.Equal(invitedUser.Id.ToString(), newIdentityUser.Id);
            Assert.Equal(invitedUser.Email, newIdentityUser.Email);
            Assert.Equal(invitedUser.Email, newIdentityUser.UserName);
            Assert.Equal(firstNameNew, newIdentityUser.FirstName);
            Assert.Equal(lastNameNew, newIdentityUser.LastName);

            var activatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == invitedUser.Id);

            // These fields should remain unchanged from the original invited User.
            Assert.Equal(invitedUser.Email, activatedUser.Email);
            Assert.Null(activatedUser.SoftDeleted);
            Assert.Null(activatedUser.DeletedById);
            Assert.Equal(invitedUser.RoleId, activatedUser.RoleId);
            activatedUser.Created.AssertUtcNow();
            Assert.Equal(activatedUser.CreatedById, activatedUser.CreatedById);
            // These three fields should have been updated. The names should have been overridden by the values from the User Profile.
            // And the Active flag should have been set to true.
            Assert.Equal(firstNameNew, activatedUser.FirstName);
            Assert.Equal(lastNameNew, activatedUser.LastName);
            Assert.True(activatedUser.Active);
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
        User softDeletedUser = _dataFixture
            .DefaultSoftDeletedUser()
            .WithRole(
                new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE",
                }
            );

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
        const string firstNameOld = "oldFirst";
        const string firstNameNew = "newFirst";
        const string lastNameOld = "oldLast";
        const string lastNameNew = "newLast";

        User userWithExpiredInvite = _dataFixture
            .DefaultUserWithExpiredInvite()
            .WithFirstName(firstNameOld)
            .WithLastName(lastNameOld)
            .WithRole(
                new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE",
                }
            );

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
                new UserProfileFromClaims(
                    Email: userWithExpiredInvite.Email,
                    FirstName: firstNameNew,
                    LastName: lastNameNew
                )
            );

        var userManager = MockUserManager();
        userManager
            .Setup(mock => mock.FindByEmailAsync(userWithExpiredInvite.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
        userReleaseRoleRepository
            .Setup(mock => mock.RemoveForUser(userWithExpiredInvite.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
        userPublicationRoleRepository
            .Setup(mock => mock.RemoveForUser(userWithExpiredInvite.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userService: userService.Object,
                userManager: userManager.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object
            );

            var result = await service.RegisterOrSignIn();

            var signInResponse = result.AssertRight();

            Assert.Equal(LoginResult.ExpiredInvite, signInResponse.LoginResult);
            Assert.Null(signInResponse.UserProfile);
        }

        VerifyAllMocks(userService, userManager, userReleaseRoleRepository, userPublicationRoleRepository);

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
            Assert.Equal(firstNameOld, user.FirstName);
            Assert.Equal(lastNameOld, user.LastName);
            Assert.False(user.Active);
            Assert.Null(user.SoftDeleted);
            Assert.Null(user.DeletedById);
            Assert.Equal(userWithExpiredInvite.RoleId, user.RoleId);
            user.Created.AssertEqual(userWithExpiredInvite.Created);
            Assert.Equal(userWithExpiredInvite.CreatedById, user.CreatedById);
        }
    }

    [Fact]
    public async Task RegisterOrSignIn_AddingRoleToUserFails_ThrowsException()
    {
        const string firstName = "new";
        const string lastName = "user";

        User invitedUser = _dataFixture
            .DefaultUserWithPendingInvite()
            .WithRole(
                new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE",
                }
            );

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.Add(invitedUser);
            await contentDbContext.SaveChangesAsync();
        }

        var logger = new Mock<ILogger<SignInService>>(Strict);
        logger.Setup(x =>
            x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (v, t) => v.ToString()!.Contains("Error adding role to invited User - unable to log in")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        );

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.GetProfileFromClaims())
            .Returns(new UserProfileFromClaims(Email: invitedUser.Email, FirstName: firstName, LastName: lastName));

        var userManager = MockUserManager();
        userManager.Setup(mock => mock.FindByEmailAsync(invitedUser.Email)).ReturnsAsync((ApplicationUser?)null);
        userManager
            .Setup(mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), invitedUser.Role!.Name!))
            .ReturnsAsync(IdentityResult.Failed());

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userManager: userManager.Object,
                userService: userService.Object,
                logger: logger.Object
            );

            await Assert.ThrowsAsync<InvalidOperationException>(service.RegisterOrSignIn);
        }

        VerifyAllMocks(logger, userManager, userService);
    }

    private static SignInService SetupService(
        ILogger<SignInService>? logger = null,
        IUserService? userService = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        UserManager<ApplicationUser>? userManager = null,
        ContentDbContext? contentDbContext = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new SignInService(
            logger: logger ?? Mock.Of<ILogger<SignInService>>(Strict),
            userService: userService ?? Mock.Of<IUserService>(Strict),
            usersAndRolesDbContext: usersAndRolesDbContext,
            userManager: userManager ?? MockUserManager().Object,
            contentDbContext: contentDbContext,
            userReleaseRoleRepository: userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            userPublicationRoleRepository: userPublicationRoleRepository
                ?? Mock.Of<IUserPublicationRoleRepository>(Strict)
        );
    }
}
