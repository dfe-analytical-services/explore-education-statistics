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
using System;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class SignInServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private static readonly Guid CreatedById = Guid.NewGuid();

    public class NonExistentUserTests : SignInServiceTests
    {
        [Fact]
        public async Task RegisterOrSignIn_Invited()
        {
            var email = "test@test.com";
            var firstName = "Bill";
            var lastName = "Piper";

            var userInvite = new UserInvite
            {
                Email = email,
                Role = new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE"
                },
                CreatedById = CreatedById.ToString(),
                Created = DateTime.UtcNow,
                Accepted = false
            };

            var userReleaseInvites = _dataFixture.DefaultUserReleaseInvite()
                .WithEmail(email)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion())
                .WithCreatedById(CreatedById)
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
                .ForIndex(2, s => s.SetRole(ReleaseRole.PrereleaseViewer))
                .GenerateList(3);

            var userPublicationInvites = _dataFixture.DefaultUserPublicationInvite()
                .WithEmail(email)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithCreatedById(CreatedById)
                .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.UserInvites.Add(userInvite);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(mock => mock.GetProfileFromClaims())
                .Returns(new UserProfileFromClaims(email, firstName, lastName))
                .Verifiable();

            var userManager = MockUserManager();
            userManager.Setup(mock => mock.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser?)null)
                .Verifiable();
            userManager.Setup(mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), userInvite.Role.Name))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.GetInvitesByEmail(email))
                .ReturnsAsync(userReleaseInvites)
                .Verifiable();

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.GetInvitesByEmail(email))
                .ReturnsAsync(userPublicationInvites)
                .Verifiable();

            var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(Strict);
            foreach (var userReleaseInvite in userReleaseInvites)
            {
                userReleaseRoleAndInviteManager
                    .Setup(mock => mock.Create(
                        It.IsAny<Guid>(),
                        userReleaseInvite.ReleaseVersionId,
                        userReleaseInvite.Role,
                        CreatedById))
                    .ReturnsAsync(new UserReleaseRole())
                    .Verifiable();
            }

            var userPublicationRoleAndInviteManager = new Mock<IUserPublicationRoleAndInviteManager>(Strict);
            foreach (var userPublicationInvite in userPublicationInvites)
            {
                userPublicationRoleAndInviteManager
                    .Setup(mock => mock.Create(
                        It.IsAny<Guid>(),
                        userPublicationInvite.PublicationId,
                        userPublicationInvite.Role,
                        CreatedById))
                    .ReturnsAsync(new UserPublicationRole())
                    .Verifiable();
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
                    userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object,
                    userPublicationRoleAndInviteManager: userPublicationRoleAndInviteManager.Object);

                var result = await service.RegisterOrSignIn();
                result.AssertRight();
            }

            VerifyAllMocks(
                userService,
                userManager,
                userReleaseInviteRepository,
                userPublicationInviteRepository,
                userReleaseRoleAndInviteManager,
                userPublicationRoleAndInviteManager);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var newIdentityUser = await usersAndRolesDbContext.Users
                    .SingleAsync(u => u.Email == email.ToLower());

                Assert.Equal(email.ToLower(), newIdentityUser.Email);
                Assert.Equal(email.ToLower(), newIdentityUser.UserName);
                Assert.Equal(firstName, newIdentityUser.FirstName);
                Assert.Equal(lastName, newIdentityUser.LastName);

                var newUser = await contentDbContext.Users
                    .SingleAsync(u => u.Email == email.ToLower());

                Assert.Equal(email.ToLower(), newUser.Email);
                Assert.Equal(firstName, newUser.FirstName);
                Assert.Equal(lastName, newUser.LastName);
            }
        }

        [Fact]
        public async Task RegisterOrSignIn_ExpiredInvite()
        {
            var email = "test@test.com";
            var firstName = "Bill";
            var lastName = "Piper";

            var userInvite = new UserInvite
            {
                Email = email,
                Role = new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE"
                },
                CreatedById = CreatedById.ToString(),
                Created = DateTime.UtcNow.AddDays(-UserInvite.InviteExpiryDurationDays - 1),
                Accepted = false
            };

            var userReleaseInvites = _dataFixture.DefaultUserReleaseInvite()
                .WithEmail(email)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion())
                .WithCreatedById(CreatedById)
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
                .ForIndex(2, s => s.SetRole(ReleaseRole.PrereleaseViewer))
                .GenerateList(3);

            var userPublicationInvites = _dataFixture.DefaultUserPublicationInvite()
                .WithEmail(email)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithCreatedById(CreatedById)
                .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.UserInvites.Add(userInvite);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(mock => mock.GetProfileFromClaims())
                .Returns(new UserProfileFromClaims(email, firstName, lastName))
                .Verifiable();

            var userManager = MockUserManager();
            userManager.Setup(mock => mock.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser?)null)
                .Verifiable();

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userService: userService.Object,
                    userManager: userManager.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userPublicationInviteRepository: userPublicationInviteRepository.Object);

                var result = await service.RegisterOrSignIn();
                result.AssertRight();
            }

            VerifyAllMocks(
                userService,
                userManager,
                userReleaseInviteRepository,
                userPublicationInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var newIdentityUser = await usersAndRolesDbContext.Users
                    .SingleOrDefaultAsync(u => u.Email == email.ToLower());

                Assert.Null(newIdentityUser);

                var newUser = await contentDbContext.Users
                    .SingleOrDefaultAsync(u => u.Email == email.ToLower());

                Assert.Null(newUser);

                var updatedUserInvite = usersAndRolesDbContext.UserInvites
                    .IgnoreQueryFilters() // Retrieve expired invites as well as active ones.
                    .Where(i => i.Email.ToLower() == email.ToLower())
                    .SingleOrDefault();

                Assert.Null(updatedUserInvite);
            }
        }
    }

    private static SignInService SetupService(
        IUserService? userService = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        UserManager<ApplicationUser>? userManager = null,
        ContentDbContext? contentDbContext = null,
        IUserReleaseRoleAndInviteManager? userReleaseRoleAndInviteManager = null,
        IUserPublicationRoleAndInviteManager? userPublicationRoleAndInviteManager = null,
        IUserReleaseInviteRepository? userReleaseInviteRepository = null,
        IUserPublicationInviteRepository? userPublicationInviteRepository = null)
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new SignInService(
            logger: Mock.Of<ILogger<SignInService>>(),
            userService: userService ?? Mock.Of<IUserService>(),
            usersAndRolesDbContext: usersAndRolesDbContext,
            userManager: userManager ?? MockUserManager().Object,
            contentDbContext: contentDbContext,
            userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager ?? Mock.Of<IUserReleaseRoleAndInviteManager>(),
            userPublicationRoleAndInviteManager: userPublicationRoleAndInviteManager ?? Mock.Of<IUserPublicationRoleAndInviteManager>(),
            userReleaseInviteRepository: userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(),
            userPublicationInviteRepository: userPublicationInviteRepository ?? Mock.Of<IUserPublicationInviteRepository>()
        );
    }
}
