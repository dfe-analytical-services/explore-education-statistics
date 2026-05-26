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

            var globalRoleService = new Mock<IGlobalRoleService>(Strict);
            globalRoleService
                .Setup(mock => mock.UpgradeToGlobalRoleIfRequired(ItIsUser(user), RoleNames.Analyst))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasRoleOnPublication(
                        _user.Id,
                        publication.Id,
                        PublicationRole.Approver,
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
                        PublicationRole.Approver,
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
                    globalRoleService: globalRoleService.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Approver);

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, globalRoleService, userPublicationRoleRepository);
        }

        [Fact]
        public async Task AddPublicationRole_Approver_UserAlreadyHasApproverRole_BadRequest()
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
                        PublicationRole.Approver,
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

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Approver);

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
        public async Task AddPublicationRole_Drafter_UserAlreadyHasApproverRole_BadRequest()
        {
            var applicationUser = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            var user = _dataFixture
                .DefaultUser()
                .WithId(Guid.Parse(applicationUser.Id))
                .WithEmail(applicationUser.Email);
            Publication publication = _dataFixture.DefaultPublication();

            UserPublicationRole existingUserPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Approver);

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(applicationUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, existingUserPublicationRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Drafter);

                result.AssertBadRequest(UserAlreadyHasMorePowerfulRole);
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
        public async Task AddPublicationRole_Drafter_UserAlreadyHasDrafterRole_BadRequest()
        {
            var applicationUser = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            var user = _dataFixture
                .DefaultUser()
                .WithId(Guid.Parse(applicationUser.Id))
                .WithEmail(applicationUser.Email);
            Publication publication = _dataFixture.DefaultPublication();

            UserPublicationRole existingUserPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter);

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(applicationUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, existingUserPublicationRole);

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

            var globalRoleService = new Mock<IGlobalRoleService>(Strict);
            globalRoleService
                .Setup(mock => mock.UpgradeToGlobalRoleIfRequired(ItIsUser(user), RoleNames.Analyst))
                .Returns(Task.CompletedTask);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(_user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_user);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(_user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_user);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, []);
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
                    globalRoleService: globalRoleService.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteDrafter(_user.Email, publication.Id);

                result.AssertRight();
            }

            VerifyAllMocks(
                userResourceRoleNotificationService,
                globalRoleService,
                userPublicationRoleRepository,
                userRepository
            );
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task UserAlreadyHasDrafterRole_BadRequest(Func<DataFixture, User> userFactory)
        {
            User existingUser = userFactory(_dataFixture);
            Publication publication = _dataFixture.DefaultPublication();

            UserPublicationRole existingUserPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(existingUser)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(existingUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, existingUserPublicationRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteDrafter(_user.Email, publication.Id);

                result.AssertBadRequest(UserAlreadyHasResourceRole);
            }

            VerifyAllMocks(userPublicationRoleRepository, userRepository);
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task UserAlreadyHasApproverRole_BadRequest(Func<DataFixture, User> userFactory)
        {
            User existingUser = userFactory(_dataFixture);
            Publication publication = _dataFixture.DefaultPublication();

            UserPublicationRole existingUserPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(existingUser)
                .WithPublication(publication)
                .WithRole(PublicationRole.Approver);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(existingUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, existingUserPublicationRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteDrafter(_user.Email, publication.Id);

                result.AssertBadRequest(UserAlreadyHasMorePowerfulRole);
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
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, []);
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

    public class GetPublicationRoleInvitesForPublicationTests : UserRoleServiceTests
    {
        [Fact]
        public async Task Success()
        {
            // Give three predictable email so that we can check the ordering
            User pendingUserInvite1 = _dataFixture.DefaultUserWithPendingInvite().WithEmail("a@example.com");
            User pendingUserInvite2 = _dataFixture.DefaultUserWithPendingInvite().WithEmail("b@example.com");
            User pendingUserInvite3 = _dataFixture.DefaultUserWithPendingInvite().WithEmail("c@example.com");
            ;
            Publication publication = _dataFixture.DefaultPublication();

            UserPublicationRole userPublicationRole1 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(pendingUserInvite1)
                .WithPublication(publication);

            UserPublicationRole userPublicationRole2 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(pendingUserInvite2)
                .WithPublication(publication);

            UserPublicationRole userPublicationRole3 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(pendingUserInvite3)
                .WithPublication(publication);

            // Role assignment for a different publication
            UserPublicationRole userPublicationRole4 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(pendingUserInvite1)
                .WithPublication(_dataFixture.DefaultPublication());

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(
                ResourceRoleFilter.PendingOnly,
                [userPublicationRole2, userPublicationRole3, userPublicationRole1, userPublicationRole4]
            );

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.GetPublicationRoleInvitesForPublication(publication.Id);

                result.AssertRight();

                var userPublicationRoles = result.Right;
                Assert.Equal(3, userPublicationRoles.Count);

                // Should be ordered by email address ascending
                Assert.Equal(userPublicationRole1.Id, userPublicationRoles[0].RoleId);
                Assert.Equal(userPublicationRole1.Role, userPublicationRoles[0].Role);
                Assert.Equal(pendingUserInvite1.Email, userPublicationRoles[0].Email);
                Assert.Equal(pendingUserInvite1.Id, userPublicationRoles[0].UserId);

                Assert.Equal(userPublicationRole2.Id, userPublicationRoles[1].RoleId);
                Assert.Equal(userPublicationRole2.Role, userPublicationRoles[1].Role);
                Assert.Equal(pendingUserInvite2.Email, userPublicationRoles[1].Email);
                Assert.Equal(pendingUserInvite2.Id, userPublicationRoles[1].UserId);

                Assert.Equal(userPublicationRole3.Id, userPublicationRoles[2].RoleId);
                Assert.Equal(userPublicationRole3.Role, userPublicationRoles[2].Role);
                Assert.Equal(pendingUserInvite3.Email, userPublicationRoles[2].Email);
                Assert.Equal(pendingUserInvite3.Id, userPublicationRoles[2].UserId);

                VerifyAllMocks(userPublicationRoleRepository);
            }
        }

        [Fact]
        public async Task NoPublication_ReturnsNotFound()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupService(contentDbContext: contentDbContext);

            var result = await service.GetPublicationRoleInvitesForPublication(Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    public class RemoveUserPublicationRoleTests : UserRoleServiceTests
    {
        [Fact]
        public async Task UserActive()
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

            var globalRoleService = new Mock<IGlobalRoleService>(Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

            globalRoleService
                .Setup(mock => mock.DowngradeFromGlobalRoleIfRequired(ItIsUser(identityUser), RoleNames.Analyst))
                .Returns(Task.CompletedTask);

            userPublicationRoleRepository
                .Setup(m => m.GetById(userPublicationRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPublicationRole);
            userPublicationRoleRepository.Setup(m => m.RemoveById(userPublicationRole.Id, default)).ReturnsAsync(true);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    globalRoleService: globalRoleService.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(globalRoleService, userPublicationRoleRepository);
        }

        [Theory]
        [MemberData(nameof(AllTypesOfNonActiveUser))]
        public async Task UserNotActive_DoesNotTryToChangeGlobalRole(Func<DataFixture, User> userFactory)
        {
            User user = userFactory(_dataFixture);

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

            userPublicationRoleRepository
                .Setup(m => m.GetById(userPublicationRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPublicationRole);
            userPublicationRoleRepository.Setup(m => m.RemoveById(userPublicationRole.Id, default)).ReturnsAsync(true);

            var service = SetupService(userPublicationRoleRepository: userPublicationRoleRepository.Object);

            var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

            result.AssertRight();

            VerifyAllMocks(userPublicationRoleRepository);
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
    }

    public class RemoveDrafterTests : UserRoleServiceTests
    {
        [Fact]
        public async Task UserActive()
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

            var globalRoleService = new Mock<IGlobalRoleService>(Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

            globalRoleService
                .Setup(mock => mock.DowngradeFromGlobalRoleIfRequired(ItIsUser(identityUser), RoleNames.Analyst))
                .Returns(Task.CompletedTask);

            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, userPublicationRole);
            userPublicationRoleRepository.Setup(m => m.RemoveById(userPublicationRole.Id, default)).ReturnsAsync(true);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    globalRoleService: globalRoleService.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.RemoveDrafter(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(globalRoleService, userPublicationRoleRepository);
        }

        [Theory]
        [MemberData(nameof(AllTypesOfNonActiveUser))]
        public async Task UserNotActive_DoesNotTryToChangeGlobalRole(Func<DataFixture, User> userFactory)
        {
            User user = userFactory(_dataFixture);

            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, userPublicationRole);
            userPublicationRoleRepository.Setup(m => m.RemoveById(userPublicationRole.Id, default)).ReturnsAsync(true);

            var service = SetupService(userPublicationRoleRepository: userPublicationRoleRepository.Object);

            var result = await service.RemoveDrafter(userPublicationRole.Id);

            result.AssertRight();

            VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task NoUserPublicationRole_ReturnsNotFound()
        {
            var userPublicationRoleGuid = Guid.NewGuid();

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, []);

            var service = SetupService(userPublicationRoleRepository: userPublicationRoleRepository.Object);

            var result = await service.RemoveDrafter(userPublicationRoleGuid);

            result.AssertNotFound();

            VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task PublicationRoleExistsButItIsNotADrafterRole_ReturnsNotFound()
        {
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Approver);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, userPublicationRole);

            var service = SetupService(userPublicationRoleRepository: userPublicationRoleRepository.Object);

            var result = await service.RemoveDrafter(userPublicationRole.Id);

            result.AssertNotFound();

            VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task RoleRemovalFails_Throws()
        {
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, userPublicationRole);
            userPublicationRoleRepository.Setup(m => m.RemoveById(userPublicationRole.Id, default)).ReturnsAsync(false);

            var service = SetupService(userPublicationRoleRepository: userPublicationRoleRepository.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RemoveDrafter(userPublicationRole.Id)
            );

            VerifyAllMocks(userPublicationRoleRepository);
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
        IUserService? userService = null,
        IGlobalRoleService? globalRoleService = null
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
            userManager ?? MockUserManager().Object,
            globalRoleService ?? Mock.Of<IGlobalRoleService>(Strict)
        );
    }
}
