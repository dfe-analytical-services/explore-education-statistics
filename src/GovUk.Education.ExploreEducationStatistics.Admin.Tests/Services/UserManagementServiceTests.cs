#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Moq;
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

    public static readonly TheoryData<DateTime?> InviteUserOptionalCreatedDates = [null, DateTime.UtcNow.AddDays(-5)];

    public class GetUserTests : UserManagementServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString(),
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "Analyst",
            };

            var globalRoles = ListOf(
                new RoleViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role 1",
                    NormalizedName = "ROLE 2",
                },
                new RoleViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role 2",
                    NormalizedName = "ROLE 2",
                }
            );

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

            var prereleaseRoles = ListOf(
                new UserPrereleaseRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 1",
                    Release = "December 2020",
                },
                new UserPrereleaseRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 2",
                    Release = "June 2021",
                }
            );

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddRangeAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userRoleService = new Mock<IUserRoleService>(Strict);
            var preReleaseUserService = new Mock<IPreReleaseUserService>(Strict);

            userRoleService.Setup(mock => mock.GetGlobalRolesForUser(user.Id)).ReturnsAsync(globalRoles);

            userRoleService.Setup(mock => mock.GetPublicationRolesForUser(userId)).ReturnsAsync(publicationRoles);

            preReleaseUserService.Setup(mock => mock.GetPrereleaseRolesForUser(userId)).ReturnsAsync(prereleaseRoles);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userRoleService: userRoleService.Object,
                    preReleaseUserService: preReleaseUserService.Object
                );

                var result = await service.GetUser(userId);

                VerifyAllMocks(userRoleService, preReleaseUserService);

                Assert.True(result.IsRight);
                var userViewModel = result.Right;

                Assert.Equal(userId, userViewModel.Id);
                Assert.Equal("test@test.com", userViewModel.Email);
                Assert.Equal("Test Analyst", userViewModel.Name);
                // Currently we only allow a user to have a maximum of one global role
                Assert.Equal(globalRoles[0].Id, userViewModel.Role);
                Assert.Equal(publicationRoles, userViewModel.UserPublicationRoles);
                Assert.Equal(prereleaseRoles, userViewModel.UserPrereleaseRoles);

                userRoleService.Verify(mock => mock.GetGlobalRolesForUser(user.Id), Times.Once);
                userRoleService.Verify(mock => mock.GetPublicationRolesForUser(userId), Times.Once);
                preReleaseUserService.Verify(mock => mock.GetPrereleaseRolesForUser(userId), Times.Once);
            }
        }

        [Fact]
        public async Task NoUser_ReturnsNotFound()
        {
            var userRoleService = new Mock<IUserRoleService>(Strict);

            await using var userAndRolesDbContext = InMemoryUserAndRolesDbContext();

            var service = SetupService(
                usersAndRolesDbContext: userAndRolesDbContext,
                userRoleService: userRoleService.Object
            );

            var result = await service.GetUser(Guid.NewGuid());

            VerifyAllMocks(userRoleService);

            result.AssertNotFound();
        }
    }

    public class ListPendingInvitesTests : UserManagementServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var identityRole = new IdentityRole { Name = GlobalRoles.RoleNames.Analyst };

            // The last pending user invite will have no associated roles
            var pendingUserInvites = _dataFixture.DefaultUserWithPendingInvite().WithRole(identityRole).GenerateList(4);

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var userPrereleaseRoles = _dataFixture
                .DefaultUserPrereleaseRole()
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

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(Strict);
            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, [.. userPrereleaseRoles]);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, false, [.. userPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
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
                var expectedUserPrereleaseRoles1 = CreateUserPrereleaseRoleViewModels(userPrereleaseRoles[..2]);

                Assert.Equal(expectedUserInvite1.Email, pendingInvite1.Email);
                Assert.Equal(expectedUserInvite1.Role!.Name, pendingInvite1.Role);
                Assert.Equal(expectedUserPublicationRoles1, pendingInvite1.UserPublicationRoles);
                Assert.Equal(expectedUserPrereleaseRoles1, pendingInvite1.UserPrereleaseRoles);

                var expectedUserInvite2 = pendingUserInvites[1];
                var pendingInvite2 = pendingInvites.Single(pi => pi.Email == expectedUserInvite2.Email);

                var expectedUserPublicationRoles2 = CreateUserPublicationRoleViewModels(userPublicationRoles[2..4]);
                var expectedUserPrereleaseRoles2 = CreateUserPrereleaseRoleViewModels(userPrereleaseRoles[2..4]);

                Assert.Equal(expectedUserInvite2.Email, pendingInvite2.Email);
                Assert.Equal(expectedUserInvite2.Role!.Name, pendingInvite2.Role);
                Assert.Equal(expectedUserPublicationRoles2, pendingInvite2.UserPublicationRoles);
                Assert.Equal(expectedUserPrereleaseRoles2, pendingInvite2.UserPrereleaseRoles);

                var expectedUserInvite3 = pendingUserInvites[2];
                var pendingInvite3 = pendingInvites.Single(pi => pi.Email == expectedUserInvite3.Email);

                var expectedUserPublicationRoles3 = CreateUserPublicationRoleViewModels(userPublicationRoles[4..6]);
                var expectedUserPrereleaseRoles3 = CreateUserPrereleaseRoleViewModels(userPrereleaseRoles[4..6]);

                Assert.Equal(expectedUserInvite3.Email, pendingInvite3.Email);
                Assert.Equal(expectedUserInvite3.Role!.Name, pendingInvite3.Role);
                Assert.Equal(expectedUserPublicationRoles3, pendingInvite3.UserPublicationRoles);
                Assert.Equal(expectedUserPrereleaseRoles3, pendingInvite3.UserPrereleaseRoles);

                // This user has no associated roles
                var expectedUserInvite4 = pendingUserInvites[3];
                var pendingInvite4 = pendingInvites.Single(pi => pi.Email == expectedUserInvite4.Email);

                Assert.Equal(expectedUserInvite4.Email, pendingInvite4.Email);
                Assert.Equal(expectedUserInvite4.Role!.Name, pendingInvite4.Role);
                Assert.Empty(pendingInvite4.UserPublicationRoles);
                Assert.Empty(pendingInvite4.UserPrereleaseRoles);
            }

            VerifyAllMocks(userPrereleaseRoleRepository, userPublicationRoleRepository);
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

        private static List<UserPrereleaseRoleViewModel> CreateUserPrereleaseRoleViewModels(
            List<UserReleaseRole> userPrereleaseRoles
        )
        {
            return
            [
                .. userPrereleaseRoles.Select(urr => new UserPrereleaseRoleViewModel
                {
                    Id = urr.Id,
                    Publication = urr.ReleaseVersion.Release.Publication.Title,
                    Release = urr.ReleaseVersion.Release.Title,
                }),
            ];
        }
    }

    public class UpdateUserTests : UserManagementServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 2",
                NormalizedName = "ROLE 2",
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddRangeAsync(user);
                await userAndRolesDbContext.AddRangeAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userRoleService = new Mock<IUserRoleService>(Strict);

            userRoleService.Setup(mock => mock.SetGlobalRoleForUser(user.Id, role.Id)).ReturnsAsync(Unit.Instance);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userRoleService: userRoleService.Object
                );

                var result = await service.UpdateUser(user.Id, role.Id);

                VerifyAllMocks(userRoleService);

                Assert.True(result.IsRight);

                userRoleService.Verify(mock => mock.SetGlobalRoleForUser(user.Id, role.Id), Times.Once);
            }
        }
    }

    public class InviteUserTests : UserManagementServiceTests
    {
        [Theory]
        [MemberData(nameof(InviteUserOptionalCreatedDates))]
        public async Task Success(DateTime? createdDate)
        {
            var email = "TEST@test.com";

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1",
            };

            User userToCreate = _dataFixture
                .DefaultUserWithPendingInvite()
                .WithEmail(email.ToLower())
                .WithCreated(createdDate ?? DateTimeOffset.UtcNow)
                .WithCreatedById(CreatedById)
                .WithRoleId(role.Id);

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            var publicationRole = PublicationRole.Drafter;
            var userPrereleaseRoles = ListOf(new UserPrereleaseRoleCreateRequest { ReleaseId = release.Id });
            var userPublicationRoles = ListOf(
                new UserPublicationRoleCreateRequest
                {
                    PublicationId = publication.Id,
                    PublicationRole = publicationRole,
                }
            );

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(email.ToLower(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(
                        email.ToLower(),
                        role.Id,
                        CreatedById,
                        createdDate,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(userToCreate);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(Strict);
            userPrereleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(userToCreate.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.Create(
                        userToCreate.Id,
                        releaseVersion.Id,
                        CreatedById,
                        createdDate,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(It.IsAny<UserReleaseRole>());

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(userToCreate.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        It.Is<HashSet<UserPublicationRoleCreateDto>>(l =>
                            l.Count == 1
                            && l.Any(upr =>
                                upr.UserId == userToCreate.Id
                                && upr.PublicationId == publication.Id
                                && upr.Role == publicationRole
                                && createdDate.HasValue
                                    ? upr.CreatedDate == createdDate
                                    : Math.Abs((upr.CreatedDate!.Value - DateTime.UtcNow).Milliseconds)
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

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.Roles.Add(role);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object
                );

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = email,
                    RoleId = role.Id,
                    UserPrereleaseRoles = userPrereleaseRoles,
                    UserPublicationRoles = userPublicationRoles,
                    CreatedDate = createdDate,
                };

                var result = await service.InviteUser(inviteRequest);

                var invitedUser = result.AssertRight();

                Assert.Equal(email.ToLower(), invitedUser.Email);
                Assert.Null(invitedUser.FirstName);
                Assert.Null(invitedUser.LastName);
                Assert.Equal(role.Id, invitedUser.RoleId);
                Assert.False(invitedUser.Active);
                Assert.Null(invitedUser.SoftDeleted);
                Assert.Null(invitedUser.DeletedById);
                invitedUser.Created.AssertEqual(createdDate ?? DateTimeOffset.UtcNow);
                Assert.Equal(CreatedById, invitedUser.CreatedById);
            }

            VerifyAllMocks(
                userPrereleaseRoleRepository,
                userPublicationRoleRepository,
                userRepository,
                userResourceRoleNotificationService
            );
        }

        [Fact]
        public async Task MultipleReleaseAndPublicationRoles()
        {
            var email = "test@test.com";

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1",
            };

            User userToCreate = _dataFixture
                .DefaultUserWithPendingInvite()
                .WithEmail(email.ToLower())
                .WithCreatedById(CreatedById)
                .WithRoleId(role.Id);

            var publications = _dataFixture
                .DefaultPublication()
                .ForRange(..2, s => s.SetReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]))
                .GenerateList(4);

            var release1 = publications[0].Releases.Single();
            var releaseVersion1 = release1.Versions.Single();

            var release2 = publications[1].Releases.Single();
            var releaseVersion2 = release2.Versions.Single();

            var publicationRole1 = PublicationRole.Drafter;
            var publicationRole2 = PublicationRole.Approver;

            var userPrereleaseRoles = new List<UserPrereleaseRoleCreateRequest>()
            {
                new() { ReleaseId = release1.Id },
                new() { ReleaseId = release2.Id },
            };
            var userPublicationRoles = new List<UserPublicationRoleCreateRequest>()
            {
                new() { PublicationId = publications[2].Id, PublicationRole = publicationRole1 },
                new() { PublicationId = publications[3].Id, PublicationRole = publicationRole2 },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.Roles.Add(role);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publications);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(email.ToLower(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(email.ToLower(), role.Id, CreatedById, null, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(userToCreate);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(Strict);
            userPrereleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(userToCreate.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.Create(userToCreate.Id, releaseVersion1.Id, CreatedById, null, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(It.IsAny<UserReleaseRole>());
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.Create(userToCreate.Id, releaseVersion2.Id, CreatedById, null, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(It.IsAny<UserReleaseRole>());

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(userToCreate.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        It.Is<HashSet<UserPublicationRoleCreateDto>>(l =>
                            l.Count == 2
                            && l.All(upr => upr.UserId == userToCreate.Id && upr.CreatedById == CreatedById)
                            && l.Any(upr =>
                                upr.PublicationId == publications[2].Id
                                && upr.Role == publicationRole1
                                && Math.Abs((upr.CreatedDate!.Value - DateTime.UtcNow).Milliseconds)
                                    <= AssertExtensions.TimeWithinMillis
                            )
                            && l.Any(upr =>
                                upr.PublicationId == publications[3].Id
                                && upr.Role == publicationRole2
                                && Math.Abs((upr.CreatedDate!.Value - DateTime.UtcNow).Milliseconds)
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
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object
                );

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = email,
                    RoleId = role.Id,
                    UserPrereleaseRoles = userPrereleaseRoles,
                    UserPublicationRoles = userPublicationRoles,
                };

                var result = await service.InviteUser(inviteRequest);

                var invitedUser = result.AssertRight();

                Assert.Equal(email.ToLower(), invitedUser.Email);
                Assert.Null(invitedUser.FirstName);
                Assert.Null(invitedUser.LastName);
                Assert.Equal(role.Id, invitedUser.RoleId);
                Assert.False(invitedUser.Active);
                Assert.Null(invitedUser.SoftDeleted);
                Assert.Null(invitedUser.DeletedById);
                invitedUser.Created.AssertEqual(DateTimeOffset.UtcNow);
                Assert.Equal(CreatedById, invitedUser.CreatedById);
            }

            VerifyAllMocks(
                userPrereleaseRoleRepository,
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
                RoleId = Guid.NewGuid().ToString(),
                UserPrereleaseRoles = [],
                UserPublicationRoles = [],
            };

            var result = await service.InviteUser(inviteRequest);

            result.AssertBadRequest(ValidationErrorMessages.UserAlreadyExists);

            VerifyAllMocks(userRepository);
        }

        [Fact]
        public async Task InvalidUserRole_ReturnsBadRequest()
        {
            var email = "test@test.com";

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = SetupService(userRepository: userRepository.Object);

            var inviteRequest = new UserInviteCreateRequest
            {
                Email = email,
                RoleId = Guid.NewGuid().ToString(),
                UserPrereleaseRoles = [],
                UserPublicationRoles = [],
            };

            var result = await service.InviteUser(inviteRequest);

            result.AssertBadRequest(ValidationErrorMessages.InvalidUserRole);

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

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(Strict);
            userPrereleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(userToCancelInvitesFor.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(userToCancelInvitesFor.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

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
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.CancelInvite(userToCancelInvitesFor.Email);

                result.AssertRight();
            }

            VerifyAllMocks(userPrereleaseRoleRepository, userPublicationRoleRepository, userRepository);
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

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(Strict);
            userPrereleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(internalUser.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(internalUser.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userManager: userManager.Object,
                    userRepository: userRepository.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.DeleteUser(internalUser.Email);
                result.AssertRight();
            }

            VerifyAllMocks(userManager, userRepository, userPrereleaseRoleRepository, userPublicationRoleRepository);
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
        IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
        IUserRoleService? userRoleService = null,
        IUserRepository? userRepository = null,
        IUserService? userService = null,
        IUserPrereleaseRoleRepository? userPrereleaseRoleRepository = null,
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
            usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
            userRoleService ?? Mock.Of<IUserRoleService>(Strict),
            userRepository ?? Mock.Of<IUserRepository>(Strict),
            userService ?? AlwaysTrueUserService(CreatedById).Object,
            userPrereleaseRoleRepository ?? Mock.Of<IUserPrereleaseRoleRepository>(Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(Strict),
            preReleaseUserService ?? Mock.Of<IPreReleaseUserService>(Strict),
            userManager ?? MockUserManager().Object
        );
    }
}
