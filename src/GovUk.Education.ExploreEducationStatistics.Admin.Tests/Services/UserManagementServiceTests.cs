#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPreReleaseRoleRepository;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPublicationRoleRepository;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserManagementServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private static readonly Guid CreatedById = Guid.NewGuid();

    public static readonly TheoryData<DateTimeOffset?> InviteUserOptionalCreatedDates =
    [
        null,
        DateTime.UtcNow.AddDays(-5),
    ];

    public class ListAllUsersTests : UserManagementServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var (user1, user2, user3) = _dataFixture
                .DefaultUser()
                .ForIndex(0, s => s.SetFirstName("User 2").SetRoleId(GlobalRoles.Role.StandardUser.GetEnumValue()))
                .ForIndex(1, s => s.SetFirstName("User 3").SetRoleId(GlobalRoles.Role.StandardUser.GetEnumValue()))
                .ForIndex(2, s => s.SetFirstName("User 1").SetRoleId(GlobalRoles.Role.BauUser.GetEnumValue()))
                .GenerateTuple3();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user1, user2, user3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext);

                var result = await service.ListAllUsers();

                var users = result.AssertRight();

                Assert.Equal(3, users.Count);
                // Should be ordered by Name
                AssertUser(user3, users[0]);
                AssertUser(user1, users[1]);
                AssertUser(user2, users[2]);
            }
        }

        private static void AssertUser(User expected, UserViewModel actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.DisplayName, actual.Name);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.GetGlobalRole(), actual.GlobalRole);
        }
    }

    public class GetUserTests : UserManagementServiceTests
    {
        [Theory]
        [InlineData(GlobalRoles.Role.StandardUser)]
        [InlineData(GlobalRoles.Role.BauUser)]
        public async Task Success(GlobalRoles.Role globalRole)
        {
            User user = _dataFixture.DefaultUser().WithRoleId(globalRole.GetEnumValue());

            var publicationRoles = ListOf(
                new UserPublicationRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 1",
                    Role = PublicationRole.Drafter,
                },
                new UserPublicationRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 2",
                    Role = PublicationRole.Drafter,
                }
            );

            var preReleaseRoles = ListOf(
                new UserPreReleaseRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 1",
                    Release = "December 2020",
                },
                new UserPreReleaseRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 2",
                    Release = "June 2021",
                }
            );

            var userRepository = new Mock<IUserRepository>(Strict);
            var userRoleService = new Mock<IUserRoleService>(Strict);
            var preReleaseUserService = new Mock<IPreReleaseUserService>(Strict);

            userRepository
                .Setup(mock => mock.FindActiveUserById(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            userRoleService.Setup(mock => mock.GetPublicationRolesForUser(user.Id)).ReturnsAsync(publicationRoles);

            preReleaseUserService.Setup(mock => mock.GetPreReleaseRolesForUser(user.Id)).ReturnsAsync(preReleaseRoles);

            var service = SetupService(
                userRoleService: userRoleService.Object,
                userRepository: userRepository.Object,
                preReleaseUserService: preReleaseUserService.Object
            );

            var result = await service.GetUser(user.Id);

            VerifyAllMocks(userRoleService, userRepository, preReleaseUserService);

            Assert.True(result.IsRight);
            var userViewModel = result.Right;

            Assert.Equal(user.Id, userViewModel.Id);
            Assert.Equal(user.Email, userViewModel.Email);
            Assert.Equal(user.DisplayName, userViewModel.Name);
            Assert.Equal(globalRole, userViewModel.GlobalRole);
            Assert.Equal(publicationRoles, userViewModel.UserPublicationRoles);
            Assert.Equal(preReleaseRoles, userViewModel.UserPreReleaseRoles);
        }

        [Fact]
        public async Task NoUser_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserById(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = SetupService(userRepository: userRepository.Object);

            var result = await service.GetUser(userId);

            VerifyAllMocks(userRepository);

            result.AssertNotFound();
        }
    }

    public class ListPendingInvitesTests : UserManagementServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var identityRole = new IdentityRole { Name = GlobalRoles.RoleNames.StandardUser };

            // The last pending user invite will have no associated roles
            var pendingUserInvites = _dataFixture.DefaultUserWithPendingInvite().WithRole(identityRole).GenerateList(4);

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var userPreReleaseRoles = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .ForIndex(0, s => s.SetUser(pendingUserInvites[0]))
                .ForIndex(1, s => s.SetUser(pendingUserInvites[1]))
                .ForIndex(2, s => s.SetUser(pendingUserInvites[2]))
                .GenerateList(3);

            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithPublication(publication)
                .ForIndex(0, s => s.SetUser(pendingUserInvites[0]).SetRole(PublicationRole.Drafter))
                .ForIndex(1, s => s.SetUser(pendingUserInvites[0]).SetRole(PublicationRole.Approver))
                .ForIndex(2, s => s.SetUser(pendingUserInvites[1]).SetRole(PublicationRole.Drafter))
                .ForIndex(3, s => s.SetUser(pendingUserInvites[1]).SetRole(PublicationRole.Approver))
                .ForIndex(4, s => s.SetUser(pendingUserInvites[2]).SetRole(PublicationRole.Drafter))
                .ForIndex(5, s => s.SetUser(pendingUserInvites[2]).SetRole(PublicationRole.Approver))
                .GenerateList(6);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.Roles.Add(identityRole);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(pendingUserInvites);
                await contentDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, [.. userPreReleaseRoles]);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, [.. userPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                var result = await service.ListPendingInvites();

                var pendingInvites = result.AssertRight();

                Assert.Equal(4, pendingInvites.Count);

                // Check they are ordered by email
                Assert.True(
                    pendingInvites.Select(pi => pi.Email).SequenceEqual(pendingInvites.Select(pi => pi.Email).Order())
                );

                var expectedUserInvite1 = pendingUserInvites[0];
                var pendingInvite1 = pendingInvites.Single(pi => pi.Email == expectedUserInvite1.Email);

                var expectedUserPublicationRoles1 = CreateUserPublicationRoleViewModels(userPublicationRoles[..2]);
                var expectedUserPreReleaseRoles1 = CreateUserPreReleaseRoleViewModels([userPreReleaseRoles[0]]);

                Assert.Equal(expectedUserInvite1.Email, pendingInvite1.Email);
                Assert.Equal(expectedUserInvite1.Role!.Name, pendingInvite1.GlobalRole);
                Assert.Equal(expectedUserPublicationRoles1, pendingInvite1.UserPublicationRoles);
                Assert.Equal(expectedUserPreReleaseRoles1, pendingInvite1.UserPreReleaseRoles);

                var expectedUserInvite2 = pendingUserInvites[1];
                var pendingInvite2 = pendingInvites.Single(pi => pi.Email == expectedUserInvite2.Email);

                var expectedUserPublicationRoles2 = CreateUserPublicationRoleViewModels(userPublicationRoles[2..4]);
                var expectedUserPreReleaseRoles2 = CreateUserPreReleaseRoleViewModels([userPreReleaseRoles[1]]);

                Assert.Equal(expectedUserInvite2.Email, pendingInvite2.Email);
                Assert.Equal(expectedUserInvite2.Role!.Name, pendingInvite2.GlobalRole);
                Assert.Equal(expectedUserPublicationRoles2, pendingInvite2.UserPublicationRoles);
                Assert.Equal(expectedUserPreReleaseRoles2, pendingInvite2.UserPreReleaseRoles);

                var expectedUserInvite3 = pendingUserInvites[2];
                var pendingInvite3 = pendingInvites.Single(pi => pi.Email == expectedUserInvite3.Email);

                var expectedUserPublicationRoles3 = CreateUserPublicationRoleViewModels(userPublicationRoles[4..6]);
                var expectedUserPreReleaseRoles3 = CreateUserPreReleaseRoleViewModels([userPreReleaseRoles[2]]);

                Assert.Equal(expectedUserInvite3.Email, pendingInvite3.Email);
                Assert.Equal(expectedUserInvite3.Role!.Name, pendingInvite3.GlobalRole);
                Assert.Equal(expectedUserPublicationRoles3, pendingInvite3.UserPublicationRoles);
                Assert.Equal(expectedUserPreReleaseRoles3, pendingInvite3.UserPreReleaseRoles);

                // This user has no associated roles
                var expectedUserInvite4 = pendingUserInvites[3];
                var pendingInvite4 = pendingInvites.Single(pi => pi.Email == expectedUserInvite4.Email);

                Assert.Equal(expectedUserInvite4.Email, pendingInvite4.Email);
                Assert.Equal(expectedUserInvite4.Role!.Name, pendingInvite4.GlobalRole);
                Assert.Empty(pendingInvite4.UserPublicationRoles);
                Assert.Empty(pendingInvite4.UserPreReleaseRoles);
            }

            VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);
        }

        private static List<UserPublicationRoleViewModel> CreateUserPublicationRoleViewModels(
            List<UserPublicationRole> userPublicationRoles
        )
        {
            return
            [
                .. userPublicationRoles.Select(upr => new UserPublicationRoleViewModel
                {
                    Id = upr.Id,
                    Publication = upr.Publication.Title,
                    Role = upr.Role,
                }),
            ];
        }

        private static List<UserPreReleaseRoleViewModel> CreateUserPreReleaseRoleViewModels(
            List<UserPreReleaseRole> userPreReleaseRoles
        )
        {
            return
            [
                .. userPreReleaseRoles.Select(uprr => new UserPreReleaseRoleViewModel
                {
                    Id = uprr.Id,
                    Publication = uprr.ReleaseVersion.Release.Publication.Title,
                    Release = uprr.ReleaseVersion.Release.Title,
                }),
            ];
        }
    }

    public class UpdateUserGlobalRoleTests : UserManagementServiceTests
    {
        [Theory]
        [InlineData(GlobalRoles.Role.StandardUser, true, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.BauUser, false, GlobalRoles.Role.StandardUser)]
        public async Task Success(
            GlobalRoles.Role oldGlobalRole,
            bool upgradeToBau,
            GlobalRoles.Role expectedNewGlobalRole
        )
        {
            User activeUser = _dataFixture.DefaultUser().WithRoleId(oldGlobalRole.GetEnumValue());

            var userRepository = new Mock<IUserRepository>(Strict);

            userRepository
                .Setup(mock => mock.FindActiveUserById(activeUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            userRepository
                .Setup(mock =>
                    mock.UpdateGlobalRole(activeUser.Id, expectedNewGlobalRole, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(It.IsAny<User>());

            var service = SetupService(userRepository: userRepository.Object);

            var result = await service.UpdateUserGlobalRole(activeUser.Id, upgradeToBau);

            VerifyAllMocks(userRepository);

            result.AssertRight();
        }

        [Fact]
        public async Task UserIsAlreadyBau_TriesUpgradingToBau_Returns400()
        {
            User activeUser = _dataFixture.DefaultUser().WithRoleId(GlobalRoles.Role.BauUser.GetEnumValue());

            var userRepository = new Mock<IUserRepository>(Strict);

            userRepository
                .Setup(mock => mock.FindActiveUserById(activeUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            var service = SetupService(userRepository: userRepository.Object);

            var result = await service.UpdateUserGlobalRole(activeUser.Id, true);

            VerifyAllMocks(userRepository);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ValidationErrorMessages.UserIsAlreadyBauUser.ToString(), error.Code);
        }

        [Fact]
        public async Task UserIsNotBau_TriesDowngradingFromBau_Returns400()
        {
            User activeUser = _dataFixture.DefaultUser().WithRoleId(GlobalRoles.Role.StandardUser.GetEnumValue());

            var userRepository = new Mock<IUserRepository>(Strict);

            userRepository
                .Setup(mock => mock.FindActiveUserById(activeUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            var service = SetupService(userRepository: userRepository.Object);

            var result = await service.UpdateUserGlobalRole(activeUser.Id, false);

            VerifyAllMocks(userRepository);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ValidationErrorMessages.UserIsAlreadyStandardUser.ToString(), error.Code);
        }

        [Fact]
        public async Task ActiveUserDoesNotExist_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(Strict);

            userRepository
                .Setup(mock => mock.FindActiveUserById(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = SetupService(userRepository: userRepository.Object);

            var result = await service.UpdateUserGlobalRole(userId, false);

            result.AssertNotFound();
        }
    }

    public class InviteUserTests : UserManagementServiceTests
    {
        [Theory]
        [MemberData(nameof(InviteUserOptionalCreatedDates))]
        public async Task Success(DateTimeOffset? createdDate)
        {
            var isBau = false;
            var globalRoleToSet = GlobalRoles.Role.StandardUser;

            User userToCreate = _dataFixture
                .DefaultUserWithPendingInvite()
                .WithCreated(createdDate ?? DateTimeOffset.UtcNow)
                .WithCreatedById(CreatedById)
                .WithRoleId(globalRoleToSet.GetEnumValue());

            var (release1, release2) = _dataFixture.DefaultRelease(publishedVersions: 1).GenerateTuple2();

            var (publication1, publication2) = _dataFixture
                .DefaultPublication()
                .ForIndex(0, s => s.SetReleases([release1]))
                .ForIndex(1, s => s.SetReleases([release2]))
                .GenerateTuple2();

            var publication2Release = publication2.Releases.Single();
            var publication2ReleaseVersion = publication2Release.Versions.Single();

            var publicationRole = PublicationRole.Drafter;
            var userPreReleaseRoles = ListOf(
                new UserPreReleaseRoleCreateRequest { ReleaseId = publication2Release.Id }
            );
            var userPublicationRoles = ListOf(
                new UserPublicationRoleCreateRequest
                {
                    PublicationId = publication1.Id,
                    PublicationRole = publicationRole,
                }
            );

            var contentDbContextId = Guid.NewGuid().ToString();

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(userToCreate.Email.ToLower(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(
                        userToCreate.Email.ToLower(),
                        CreatedById,
                        globalRoleToSet,
                        createdDate,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(userToCreate);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        It.Is<HashSet<UserPreReleaseRoleCreateDto>>(l =>
                            l.Count == 1
                            && l.Any(uprr =>
                                uprr.UserId == userToCreate.Id
                                && uprr.ReleaseVersionId == publication2ReleaseVersion.Id
                                && createdDate.HasValue
                                    ? uprr.CreatedDate == createdDate
                                    : Math.Abs((uprr.CreatedDate - DateTime.UtcNow).Milliseconds)
                                        <= AssertExtensions.TimeWithinMillis
                                        && uprr.CreatedById == CreatedById
                            )
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync([]); // Don't actually need to return anything here for the test. Just want to check it was called correctly.

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        It.Is<HashSet<UserPublicationRoleCreateDto>>(l =>
                            l.Count == 1
                            && l.Any(upr =>
                                upr.UserId == userToCreate.Id
                                && upr.PublicationId == publication1.Id
                                && upr.Role == publicationRole
                                && createdDate.HasValue
                                    ? upr.CreatedDate == createdDate
                                    : Math.Abs((upr.CreatedDate - DateTime.UtcNow).Milliseconds)
                                        <= AssertExtensions.TimeWithinMillis
                                        && upr.CreatedById == CreatedById
                            )
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync([]); // Don't actually need to return anything here for the test. Just want to check it was called correctly.

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock => mock.NotifyUserOfInvite(userToCreate.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publication1, publication2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object
                );

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = userToCreate.Email,
                    IsBau = isBau,
                    UserPreReleaseRoles = userPreReleaseRoles,
                    UserPublicationRoles = userPublicationRoles,
                    CreatedDate = createdDate,
                };

                var result = await service.InviteUser(inviteRequest);

                var invitedUser = result.AssertRight();

                Assert.Equal(userToCreate.Email.ToLower(), invitedUser.Email);
                Assert.Null(invitedUser.FirstName);
                Assert.Null(invitedUser.LastName);
                Assert.Equal(globalRoleToSet.GetEnumValue(), invitedUser.RoleId);
                Assert.False(invitedUser.Active);
                Assert.Null(invitedUser.SoftDeleted);
                Assert.Null(invitedUser.DeletedById);
                invitedUser.Created.AssertEqual(createdDate ?? DateTimeOffset.UtcNow);
                Assert.Equal(CreatedById, invitedUser.CreatedById);
            }

            VerifyAllMocks(
                userPreReleaseRoleRepository,
                userPublicationRoleRepository,
                userRepository,
                userResourceRoleNotificationService
            );
        }

        [Theory]
        [InlineData(true, GlobalRoles.Role.BauUser)]
        [InlineData(false, GlobalRoles.Role.StandardUser)]
        public async Task SetsCorrectGlobalRole(bool isBau, GlobalRoles.Role expectedGlobalRoleToSet)
        {
            User userToCreate = _dataFixture
                .DefaultUserWithPendingInvite()
                .WithRoleId(expectedGlobalRoleToSet.GetEnumValue());

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(userToCreate.Email.ToLower(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(
                        userToCreate.Email.ToLower(),
                        CreatedById,
                        expectedGlobalRoleToSet,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(userToCreate);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        Array.Empty<UserPreReleaseRoleCreateDto>().ToHashSet(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync([]); // Don't actually need to return anything here for the test. Just want to check it was called correctly.

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        Array.Empty<UserPublicationRoleCreateDto>().ToHashSet(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync([]); // Don't actually need to return anything here for the test. Just want to check it was called correctly.

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock => mock.NotifyUserOfInvite(userToCreate.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = SetupService(
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userRepository: userRepository.Object,
                userResourceRoleNotificationService: userResourceRoleNotificationService.Object
            );

            var inviteRequest = new UserInviteCreateRequest
            {
                Email = userToCreate.Email,
                IsBau = isBau,
                UserPreReleaseRoles = [],
                UserPublicationRoles = [],
            };

            var result = await service.InviteUser(inviteRequest);

            var invitedUser = result.AssertRight();

            Assert.Equal(expectedGlobalRoleToSet.GetEnumValue(), invitedUser.RoleId);

            VerifyAllMocks(
                userPreReleaseRoleRepository,
                userPublicationRoleRepository,
                userRepository,
                userResourceRoleNotificationService
            );
        }

        [Fact]
        public async Task MultipleReleaseAndPublicationRoles()
        {
            var isBau = false;
            var globalRoleToSet = GlobalRoles.Role.StandardUser;

            User userToCreate = _dataFixture
                .DefaultUserWithPendingInvite()
                .WithCreatedById(CreatedById)
                .WithRoleId(globalRoleToSet.GetEnumValue());

            var (release1, release2, release3, release4) = _dataFixture
                .DefaultRelease(publishedVersions: 1)
                .GenerateTuple4();

            var releaseVersion1 = release1.Versions.Single();
            var releaseVersion2 = release2.Versions.Single();

            var publications = _dataFixture
                .DefaultPublication()
                .ForIndex(0, s => s.SetReleases([release1]))
                .ForIndex(1, s => s.SetReleases([release2]))
                .ForIndex(2, s => s.SetReleases([release3]))
                .ForIndex(3, s => s.SetReleases([release4]))
                .GenerateList(4);

            var publicationRole1 = PublicationRole.Drafter;
            var publicationRole2 = PublicationRole.Approver;

            var userPreReleaseRoles = new List<UserPreReleaseRoleCreateRequest>()
            {
                new() { ReleaseId = release1.Id },
                new() { ReleaseId = release2.Id },
                // These two should be ignored and not created, as the invite request includes
                // the more powerful publication roles for publications 3 and 4 (see below)
                new() { ReleaseId = release3.Id },
                new() { ReleaseId = release4.Id },
            };
            var userPublicationRoles = new List<UserPublicationRoleCreateRequest>()
            {
                new() { PublicationId = publications[2].Id, PublicationRole = publicationRole1 },
                new() { PublicationId = publications[3].Id, PublicationRole = publicationRole2 },
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publications);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(userToCreate.Email.ToLower(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(
                        userToCreate.Email.ToLower(),
                        CreatedById,
                        globalRoleToSet,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(userToCreate);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(Strict);
            // Should only try to create pre-release roles for releases 1 and 2, as releases 3 and 4 belong to
            // publications 3 and 4 which are already having more powerful publication roles created for them.
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        It.Is<HashSet<UserPreReleaseRoleCreateDto>>(l =>
                            l.Count == 2
                            && l.All(upr => upr.UserId == userToCreate.Id && upr.CreatedById == CreatedById)
                            && l.Any(upr =>
                                upr.ReleaseVersionId == releaseVersion1.Id
                                && Math.Abs((upr.CreatedDate - DateTime.UtcNow).Milliseconds)
                                    <= AssertExtensions.TimeWithinMillis
                            )
                            && l.Any(upr =>
                                upr.ReleaseVersionId == releaseVersion2.Id
                                && Math.Abs((upr.CreatedDate - DateTime.UtcNow).Milliseconds)
                                    <= AssertExtensions.TimeWithinMillis
                            )
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync([]); // Don't actually need to return anything here for the test. Just want to check it was called correctly.

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        It.Is<HashSet<UserPublicationRoleCreateDto>>(l =>
                            l.Count == 2
                            && l.All(upr => upr.UserId == userToCreate.Id && upr.CreatedById == CreatedById)
                            && l.Any(upr =>
                                upr.PublicationId == publications[2].Id
                                && upr.Role == publicationRole1
                                && Math.Abs((upr.CreatedDate - DateTime.UtcNow).Milliseconds)
                                    <= AssertExtensions.TimeWithinMillis
                            )
                            && l.Any(upr =>
                                upr.PublicationId == publications[3].Id
                                && upr.Role == publicationRole2
                                && Math.Abs((upr.CreatedDate - DateTime.UtcNow).Milliseconds)
                                    <= AssertExtensions.TimeWithinMillis
                            )
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync([]); // Don't actually need to return anything here for the test. Just want to check it was called correctly.

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock => mock.NotifyUserOfInvite(userToCreate.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object
                );

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = userToCreate.Email,
                    IsBau = isBau,
                    UserPreReleaseRoles = userPreReleaseRoles,
                    UserPublicationRoles = userPublicationRoles,
                };

                var result = await service.InviteUser(inviteRequest);

                var invitedUser = result.AssertRight();

                Assert.Equal(userToCreate.Email.ToLower(), invitedUser.Email);
                Assert.Null(invitedUser.FirstName);
                Assert.Null(invitedUser.LastName);
                Assert.Equal(globalRoleToSet.GetEnumValue(), invitedUser.RoleId);
                Assert.False(invitedUser.Active);
                Assert.Null(invitedUser.SoftDeleted);
                Assert.Null(invitedUser.DeletedById);
                invitedUser.Created.AssertEqual(DateTimeOffset.UtcNow);
                Assert.Equal(CreatedById, invitedUser.CreatedById);
            }

            VerifyAllMocks(
                userPreReleaseRoleRepository,
                userPublicationRoleRepository,
                userRepository,
                userResourceRoleNotificationService
            );
        }

        [Fact]
        public async Task ActiveUserAlreadyExists_ReturnsBadRequest()
        {
            User activeUser = _dataFixture.DefaultUser();

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(activeUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            var service = SetupService(userRepository: userRepository.Object);

            var inviteRequest = new UserInviteCreateRequest
            {
                Email = activeUser.Email,
                IsBau = false,
                UserPreReleaseRoles = [],
                UserPublicationRoles = [],
            };

            var result = await service.InviteUser(inviteRequest);

            result.AssertBadRequest(ValidationErrorMessages.UserAlreadyExists);

            VerifyAllMocks(userRepository);
        }
    }

    public class CancelInviteTests : UserManagementServiceTests
    {
        [Fact]
        public async Task Success()
        {
            User userToCancelInvitesFor = _dataFixture.DefaultUserWithPendingInvite();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock =>
                    mock.FindPendingUserInviteByEmail(userToCancelInvitesFor.Email, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(userToCancelInvitesFor);
            userRepository
                .Setup(mock =>
                    mock.SoftDeleteUser(userToCancelInvitesFor.Id, CreatedById, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userRepository: userRepository.Object
                );

                var result = await service.CancelInvite(userToCancelInvitesFor.Email);

                result.AssertRight();
            }

            VerifyAllMocks(userRepository);
        }

        [Fact]
        public async Task PendingUserInviteDoesNotExist_ValidationError()
        {
            var email = "test@test.com";

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindPendingUserInviteByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = SetupService(userRepository: userRepository.Object);

            var result = await service.CancelInvite(email);

            var actionResult = result.AssertLeft();

            actionResult.AssertValidationProblem(ValidationErrorMessages.InviteNotFound);

            VerifyAllMocks(userRepository);
        }
    }

    public class DeleteUserTests : UserManagementServiceTests
    {
        [Fact]
        public async Task Success()
        {
            User internalUser = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Email = internalUser.Email };

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.Users.Add(identityUser);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();
            userManager
                .Setup(mock => mock.DeleteAsync(It.Is<ApplicationUser>(user => user.Email == internalUser.Email)))
                .ReturnsAsync(new IdentityResult());

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(internalUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(internalUser);
            userRepository
                .Setup(mock => mock.SoftDeleteUser(internalUser.Id, CreatedById, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userManager: userManager.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.DeleteUser(internalUser.Email);
                result.AssertRight();
            }

            VerifyAllMocks(userManager, userRepository);
        }

        [Fact]
        public async Task ActiveUserDoesNotExist_ReturnsNotFound()
        {
            var email = "test@test.com";

            await using var usersAndRolesDbContext = InMemoryUserAndRolesDbContext();
            await using var contentDbContext = InMemoryApplicationDbContext();

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = SetupService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userRepository: userRepository.Object
            );

            var result = await service.DeleteUser(email);
            result.AssertNotFound();
        }

        [Fact]
        public async Task IdentityUserDoesNotExist_ReturnsNotFound()
        {
            User internalUser = _dataFixture.DefaultUser();

            await using var usersAndRolesDbContext = InMemoryUserAndRolesDbContext();
            await using var contentDbContext = InMemoryApplicationDbContext();

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(internalUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(internalUser);

            var service = SetupService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userRepository: userRepository.Object
            );

            var result = await service.DeleteUser(internalUser.Email);
            result.AssertNotFound();
        }
    }

    private static UserManagementService SetupService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IUserRoleService? userRoleService = null,
        IUserRepository? userRepository = null,
        IUserService? userService = null,
        IUserPreReleaseRoleRepository? userPreReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IPreReleaseUserService? preReleaseUserService = null,
        UserManager<ApplicationUser>? userManager = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new UserManagementService(
            usersAndRolesDbContext,
            contentDbContext,
            userRoleService ?? Mock.Of<IUserRoleService>(Strict),
            userRepository ?? Mock.Of<IUserRepository>(Strict),
            userService ?? AlwaysTrueUserService(CreatedById).Object,
            userPreReleaseRoleRepository ?? Mock.Of<IUserPreReleaseRoleRepository>(Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(Strict),
            preReleaseUserService ?? Mock.Of<IPreReleaseUserService>(Strict),
            userManager ?? MockUserManager().Object
        );
    }
}
