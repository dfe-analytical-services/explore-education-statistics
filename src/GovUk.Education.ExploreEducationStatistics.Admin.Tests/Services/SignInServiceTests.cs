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

    public class NonExistentUserTests : SignInServiceTests
    {
        [Fact]
        public async Task RegisterOrSignIn_Invited_CreatesNewUser()
        {
            var email = "test@test.com";
            var createdByEmail = "test2@test.com";
            var firstName = "Bill";
            var lastName = "Piper";

            var createdByAspNetUser = new ApplicationUser
            {
                Id = CreatedById.ToString(),
                Email = createdByEmail.ToLower(),
                UserName = createdByEmail.ToLower()
            };

            var userInvite = new UserInvite
            {
                Email = email,
                Role = new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE"
                },
                CreatedBy = createdByAspNetUser,
                Created = DateTime.UtcNow,
                Accepted = false
            };

            var createdByInternalUser = _dataFixture.DefaultUser()
                .WithEmail(createdByEmail.ToLower())
                .Generate();

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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdByInternalUser);
                await contentDbContext.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(mock => mock.GetProfileFromClaims())
                .Returns(new UserProfileFromClaims(email, firstName, lastName));

            var userManager = MockUserManager();
            userManager.Setup(mock => mock.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser?)null);
            userManager.Setup(mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), userInvite.Role.Name))
                .ReturnsAsync(IdentityResult.Success);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userReleaseInvites);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPublicationInvites);
            userPublicationInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            foreach (var userReleaseInvite in userReleaseInvites)
            {
                userReleaseRoleRepository
                    .Setup(mock => mock.Create(
                        It.IsAny<Guid>(),
                        userReleaseInvite.ReleaseVersionId,
                        userReleaseInvite.Role,
                        CreatedById))
                    .ReturnsAsync(new UserReleaseRole());
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            foreach (var userPublicationInvite in userPublicationInvites)
            {
                userPublicationRoleRepository
                    .Setup(mock => mock.Create(
                        It.IsAny<Guid>(),
                        userPublicationInvite.PublicationId,
                        userPublicationInvite.Role,
                        CreatedById))
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
                    userPublicationRoleRepository: userPublicationRoleRepository.Object);

                var result = await service.RegisterOrSignIn();
                result.AssertRight();
            }

            VerifyAllMocks(
                userService,
                userManager,
                userReleaseInviteRepository,
                userPublicationInviteRepository,
                userReleaseRoleRepository,
                userPublicationRoleRepository);

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
                Assert.True(newUser.Active);
                Assert.Equal(userInvite.RoleId, newUser.RoleId);
                newUser.Created.AssertUtcNow();
                Assert.Equal(createdByInternalUser.Id, newUser.CreatedById);
            }
        }

        [Fact]
        public async Task RegisterOrSignIn_ExpiredInvite_RemovesExpiredInvites()
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
                .Returns(new UserProfileFromClaims(email, firstName, lastName));

            var userManager = MockUserManager();
            userManager.Setup(mock => mock.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser?)null);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
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

        [Fact]
        public async Task
            RegisterOrSignIn_UserInviteMissingCreatedById_SetsUserCreatedByIdToDeletedUserPlaceholder()
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
                Created = DateTime.UtcNow,
                Accepted = false
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.UserInvites.Add(userInvite);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(mock => mock.GetProfileFromClaims())
                .Returns(new UserProfileFromClaims(email, firstName, lastName));

            var userManager = MockUserManager();
            userManager.Setup(mock => mock.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser?)null);
            userManager.Setup(mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), userInvite.Role.Name))
                .ReturnsAsync(IdentityResult.Success);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            userPublicationInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var deletedUserPlaceholderId = Guid.NewGuid();
            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository.Setup(mock => mock.FindDeletedUserPlaceholder())
               .ReturnsAsync(_dataFixture.DefaultUser()
                    .WithId(deletedUserPlaceholderId)
                    .Generate());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userService: userService.Object,
                    userManager: userManager.Object,
                    userRepository: userRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userPublicationInviteRepository: userPublicationInviteRepository.Object);

                var result = await service.RegisterOrSignIn();
                result.AssertRight();
            }

            VerifyAllMocks(
                userService,
                userManager,
                userRepository,
                userReleaseInviteRepository,
                userPublicationInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var newUser = await contentDbContext.Users
                    .SingleAsync(u => u.Email == email.ToLower());

                Assert.Equal(deletedUserPlaceholderId, newUser.CreatedById);
            }
        }
    }

    public class SoftDeletedUserTests : SignInServiceTests
    {
        [Fact]
        public async Task RegisterOrSignIn_ReactivatesUser()
        {
            var email = "test@test.com";
            var createdByEmail = "test2@test.com";
            var firstName = "Bill";
            var lastName = "Piper";
            var updatedFirstName = $"{firstName}-UPDATED";
            var updatedLastName = $"{lastName}-UPDATED";

            var createdByAspNetUser = new ApplicationUser
            {
                Id = CreatedById.ToString(),
                Email = createdByEmail.ToLower(),
                UserName = createdByEmail.ToLower()
            };

            var userInvite = new UserInvite
            {
                Email = email,
                Role = new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role",
                    NormalizedName = "ROLE"
                },
                CreatedBy = createdByAspNetUser,
                Created = DateTime.UtcNow,
                Accepted = false
            };

            var createdByInternalUser = _dataFixture.DefaultUser()
                .WithEmail(createdByEmail.ToLower())
                .Generate();

            var existingUser = _dataFixture.DefaultUser()
                .WithEmail(email.ToLower())
                .WithFirstName(firstName)
                .WithLastName(lastName)
                .WithSoftDeleted(DateTime.UtcNow.AddDays(-1))
                .WithActive(false)
                .WithCreated(DateTime.UtcNow.AddDays(-2))
                .Generate();

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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(createdByInternalUser, existingUser);
                await contentDbContext.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(mock => mock.GetProfileFromClaims())
                .Returns(new UserProfileFromClaims(email, updatedFirstName, updatedLastName));

            var userManager = MockUserManager();
            userManager.Setup(mock => mock.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser?)null);
            userManager.Setup(mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), userInvite.Role.Name))
                .ReturnsAsync(IdentityResult.Success);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userReleaseInvites);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPublicationInvites);
            userPublicationInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            foreach (var userReleaseInvite in userReleaseInvites)
            {
                userReleaseRoleRepository
                    .Setup(mock => mock.Create(
                        It.IsAny<Guid>(),
                        userReleaseInvite.ReleaseVersionId,
                        userReleaseInvite.Role,
                        CreatedById))
                    .ReturnsAsync(new UserReleaseRole());
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            foreach (var userPublicationInvite in userPublicationInvites)
            {
                userPublicationRoleRepository
                    .Setup(mock => mock.Create(
                        It.IsAny<Guid>(),
                        userPublicationInvite.PublicationId,
                        userPublicationInvite.Role,
                        CreatedById))
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
                    userPublicationRoleRepository: userPublicationRoleRepository.Object);

                var result = await service.RegisterOrSignIn();
                result.AssertRight();
            }

            VerifyAllMocks(
                userService,
                userManager,
                userReleaseInviteRepository,
                userPublicationInviteRepository,
                userReleaseRoleRepository,
                userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var newIdentityUser = await usersAndRolesDbContext.Users
                    .SingleAsync(u => u.Email == email.ToLower());

                Assert.Equal(email.ToLower(), newIdentityUser.Email);
                Assert.Equal(email.ToLower(), newIdentityUser.UserName);
                Assert.Equal(updatedFirstName, newIdentityUser.FirstName);
                Assert.Equal(updatedLastName, newIdentityUser.LastName);

                var newUser = await contentDbContext.Users
                    .IgnoreQueryFilters() // Make sure we didn't create an ADDITIONAL user, but just reactivated the existing one
                    .SingleAsync(u => u.Email == email.ToLower());

                Assert.Equal(email.ToLower(), newUser.Email);
                Assert.Equal(updatedFirstName, newUser.FirstName); // Updated to the new FirstName from the invite
                Assert.Equal(updatedLastName, newUser.LastName); // Updated to the new LastName from the invite
                Assert.Null(newUser.SoftDeleted); // Reset back to active state
                Assert.True(newUser.Active); // Reset back to active state
                Assert.Equal(userInvite.RoleId, newUser.RoleId); // Updated to the new Role from the invite
                newUser.Created.AssertUtcNow(); // Updated to now
                Assert.Equal(createdByInternalUser.Id, newUser.CreatedById); // Updated to the new CreatedById from the invite
            }
        }

        [Fact]
        public async Task RegisterOrSignIn_UserInviteMissingCreatedById_SetsUserCreatedByIdToDeletedUserPlaceholder()
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
                Created = DateTime.UtcNow,
                Accepted = false
            };

            var existingUser = _dataFixture.DefaultUser()
                .WithEmail(email.ToLower())
                .WithFirstName(firstName)
                .WithLastName(lastName)
                .WithSoftDeleted(DateTime.UtcNow.AddDays(-1))
                .WithActive(false)
                .WithCreated(DateTime.UtcNow.AddDays(-2))
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.UserInvites.Add(userInvite);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(existingUser);
                await contentDbContext.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(mock => mock.GetProfileFromClaims())
                .Returns(new UserProfileFromClaims(email, firstName, lastName));

            var userManager = MockUserManager();
            userManager.Setup(mock => mock.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser?)null);
            userManager.Setup(mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), userInvite.Role.Name))
                .ReturnsAsync(IdentityResult.Success);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            userPublicationInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var deletedUserPlaceholderId = Guid.NewGuid();
            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository.Setup(mock => mock.FindDeletedUserPlaceholder())
               .ReturnsAsync(_dataFixture.DefaultUser()
                   .WithId(deletedUserPlaceholderId)
                   .Generate());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userService: userService.Object,
                    userManager: userManager.Object,
                    userRepository: userRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userPublicationInviteRepository: userPublicationInviteRepository.Object);

                var result = await service.RegisterOrSignIn();
                result.AssertRight();
            }

            VerifyAllMocks(
                userService,
                userManager,
                userRepository,
                userReleaseInviteRepository,
                userPublicationInviteRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var newUser = await contentDbContext.Users
                    .IgnoreQueryFilters() // Make sure we didn't create an ADDITIONAL user, but just reactivated the existing one
                    .SingleAsync(u => u.Email == email.ToLower());

                Assert.Equal(deletedUserPlaceholderId, newUser.CreatedById);
            }
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
        IUserPublicationInviteRepository? userPublicationInviteRepository = null,
        IUserRepository? userRepository = null)
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
            userPublicationInviteRepository: userPublicationInviteRepository ?? Mock.Of<IUserPublicationInviteRepository>(),
            userRepository: userRepository ?? Mock.Of<IUserRepository>()
        );
    }
}
