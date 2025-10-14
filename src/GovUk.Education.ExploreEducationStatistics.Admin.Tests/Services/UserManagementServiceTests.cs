#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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

    public static readonly TheoryData<DateTime?> InviteUserOptionalCreatedDates =
    [
        null,
        DateTime.UtcNow.AddDays(-5)
    ];

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
                LastName = "Analyst"
            };

            var globalRoles = ListOf(
                new RoleViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role 1",
                    NormalizedName = "ROLE 2"
                },
                new RoleViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role 2",
                    NormalizedName = "ROLE 2"
                });

            var publicationRoles = ListOf(
                new UserPublicationRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 1",
                    Role = PublicationRole.Owner
                },
                new UserPublicationRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 2",
                    Role = PublicationRole.Owner
                });

            var releaseRoles = ListOf(
                new UserReleaseRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 1",
                    Release = "December 2020",
                    Role = ReleaseRole.Contributor
                },
                new UserReleaseRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 2",
                    Release = "June 2021",
                    Role = ReleaseRole.Approver
                });

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddRangeAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userRoleService = new Mock<IUserRoleService>(Strict);

            userRoleService.Setup(mock =>
                mock.GetGlobalRoles(user.Id)).ReturnsAsync(globalRoles);

            userRoleService.Setup(mock =>
                mock.GetPublicationRolesForUser(userId)).ReturnsAsync(publicationRoles);

            userRoleService.Setup(mock =>
                mock.GetReleaseRoles(userId)).ReturnsAsync(releaseRoles);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userRoleService: userRoleService.Object);

                var result = await service.GetUser(userId);

                VerifyAllMocks(userRoleService);

                Assert.True(result.IsRight);
                var userViewModel = result.Right;

                Assert.Equal(userId, userViewModel.Id);
                Assert.Equal("test@test.com", userViewModel.Email);
                Assert.Equal("Test Analyst", userViewModel.Name);
                // Currently we only allow a user to have a maximum of one global role
                Assert.Equal(globalRoles[0].Id, userViewModel.Role);
                Assert.Equal(publicationRoles, userViewModel.UserPublicationRoles);
                Assert.Equal(releaseRoles, userViewModel.UserReleaseRoles);

                userRoleService.Verify(mock => mock.GetGlobalRoles(user.Id), Times.Once);
                userRoleService.Verify(mock => mock.GetPublicationRolesForUser(userId), Times.Once);
                userRoleService.Verify(mock => mock.GetReleaseRoles(userId), Times.Once);
            }
        }

        [Fact]
        public async Task NoUser_ReturnsNotFound()
        {
            var userRoleService = new Mock<IUserRoleService>(Strict);

            await using var userAndRolesDbContext = InMemoryUserAndRolesDbContext();

            var service = SetupUserManagementService(
                usersAndRolesDbContext: userAndRolesDbContext,
                userRoleService: userRoleService.Object);

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

            var pendingUserInvites = _dataFixture.DefaultUser()
                .WithActive(false)
                .WithRole(identityRole)
                // This user is active and so should not be returned
                .ForIndex(3, s => s.SetActive(true))
                // This user is soft-deleted and so should not be returned
                .ForIndex(4, s => s.SetSoftDeleted(DateTime.UtcNow))
                .GenerateList(5);

            var publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                .Generate();

            var userReleaseInvites = _dataFixture.DefaultUserReleaseInvite()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .ForIndex(0, s => s.SetEmail(pendingUserInvites[0].Email))
                .ForIndex(1, s => s.SetEmail(pendingUserInvites[0].Email))
                .ForIndex(2, s => s.SetEmail(pendingUserInvites[1].Email))
                .ForIndex(3, s => s.SetEmail(pendingUserInvites[1].Email))
                .ForIndex(4, s => s.SetEmail(pendingUserInvites[2].Email))
                .ForIndex(5, s => s.SetEmail(pendingUserInvites[2].Email))
                .ForIndex(6, s => s.SetEmail(pendingUserInvites[3].Email))
                .ForIndex(7, s => s.SetEmail(pendingUserInvites[3].Email))
                .ForIndex(8, s => s.SetEmail(pendingUserInvites[4].Email))
                .ForIndex(9, s => s.SetEmail(pendingUserInvites[4].Email))
                .GenerateList(10);

            var userPublicationInvites = _dataFixture.DefaultUserPublicationInvite()
                .WithPublication(publication)
                .ForIndex(0, s => s.SetEmail(pendingUserInvites[0].Email))
                .ForIndex(1, s => s.SetEmail(pendingUserInvites[0].Email))
                .ForIndex(2, s => s.SetEmail(pendingUserInvites[1].Email))
                .ForIndex(3, s => s.SetEmail(pendingUserInvites[1].Email))
                .ForIndex(4, s => s.SetEmail(pendingUserInvites[2].Email))
                .ForIndex(5, s => s.SetEmail(pendingUserInvites[2].Email))
                .ForIndex(6, s => s.SetEmail(pendingUserInvites[3].Email))
                .ForIndex(7, s => s.SetEmail(pendingUserInvites[3].Email))
                .ForIndex(8, s => s.SetEmail(pendingUserInvites[4].Email))
                .ForIndex(9, s => s.SetEmail(pendingUserInvites[4].Email))
                .GenerateList(10);

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
                contentDbContext.UserReleaseInvites.AddRange(userReleaseInvites);
                contentDbContext.UserPublicationInvites.AddRange(userPublicationInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext);

                var result = await service.ListPendingInvites();

                var pendingInvites = result.AssertRight();

                // Check they are ordered by email
                Assert.True(
                    pendingInvites
                        .Select(pi => pi.Email)
                        .SequenceEqual(pendingInvites.Select(pi => pi.Email).OrderBy(e => e))
                );

                var expectedUserInvite1 = pendingUserInvites[0];
                var pendingInvite1 = pendingInvites.Single(pi => pi.Email == expectedUserInvite1.Email);
                
                var expectedUserPublicationRoles1 = CreateUserPublicationRoleViewModels(
                    userPublicationInvites[..2]);
                var expectedUserReleaseRoles1 = CreateUserReleaseRoleViewModels(
                    userReleaseInvites[..2]);

                Assert.Equal(expectedUserInvite1.Email, pendingInvite1.Email);
                Assert.Equal(expectedUserInvite1.Role!.Name, pendingInvite1.Role);
                Assert.Equal(expectedUserPublicationRoles1, pendingInvite1.UserPublicationRoles);
                Assert.Equal(expectedUserReleaseRoles1, pendingInvite1.UserReleaseRoles);

                var expectedUserInvite2 = pendingUserInvites[1];
                var pendingInvite2 = pendingInvites.Single(pi => pi.Email == expectedUserInvite2.Email);

                var expectedUserPublicationRoles2 = CreateUserPublicationRoleViewModels(
                    userPublicationInvites[2..4]);
                var expectedUserReleaseRoles2 = CreateUserReleaseRoleViewModels(
                    userReleaseInvites[2..4]);

                Assert.Equal(expectedUserInvite2.Email, pendingInvite2.Email);
                Assert.Equal(expectedUserInvite2.Role!.Name, pendingInvite2.Role);
                Assert.Equal(expectedUserPublicationRoles2, pendingInvite2.UserPublicationRoles);
                Assert.Equal(expectedUserReleaseRoles2, pendingInvite2.UserReleaseRoles);

                var expectedUserInvite3 = pendingUserInvites[2];
                var pendingInvite3 = pendingInvites.Single(pi => pi.Email == expectedUserInvite3.Email);

                var expectedUserPublicationRoles3 = CreateUserPublicationRoleViewModels(
                    userPublicationInvites[4..6]);
                var expectedUserReleaseRoles3 = CreateUserReleaseRoleViewModels(
                    userReleaseInvites[4..6]);

                Assert.Equal(expectedUserInvite3.Email, pendingInvite3.Email);
                Assert.Equal(expectedUserInvite3.Role!.Name, pendingInvite3.Role);
                Assert.Equal(expectedUserPublicationRoles3, pendingInvite3.UserPublicationRoles);
                Assert.Equal(expectedUserReleaseRoles3, pendingInvite3.UserReleaseRoles);
            }
        }

        private static List<UserPublicationRoleViewModel> CreateUserPublicationRoleViewModels(
            List<UserPublicationInvite> userPublicationInvites)
        {
            return [.. userPublicationInvites
                .Select(upi => new UserPublicationRoleViewModel
                {
                    Id = upi.Id,
                    Publication = upi.Publication.Title,
                    Role = upi.Role
                })];
        }

        private static List<UserReleaseRoleViewModel> CreateUserReleaseRoleViewModels(
            List<UserReleaseInvite> userReleaseInvites)
        {
            return [.. userReleaseInvites
                .Select(uri => new UserReleaseRoleViewModel
                {
                    Id = uri.Id,
                    Publication = uri.ReleaseVersion.Release.Publication.Title,
                    Release = uri.ReleaseVersion.Release.Title,
                    Role = uri.Role
                })];
        }
    }

    public class UpdateUserTests : UserManagementServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString()
            };

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 2",
                NormalizedName = "ROLE 2"
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddRangeAsync(user);
                await userAndRolesDbContext.AddRangeAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userRoleService = new Mock<IUserRoleService>(Strict);

            userRoleService.Setup(mock =>
                mock.SetGlobalRole(user.Id, role.Id)).ReturnsAsync(Unit.Instance);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userRoleService: userRoleService.Object);

                var result = await service.UpdateUser(user.Id, role.Id);

                VerifyAllMocks(userRoleService);

                Assert.True(result.IsRight);

                userRoleService.Verify(mock => mock.SetGlobalRole(
                        user.Id, role.Id),
                    Times.Once);
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
                NormalizedName = "ROLE 1"
            };

            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            var releaseRole = ReleaseRole.Approver;
            var publicationRole = PublicationRole.Owner;
            var userReleaseRoles = ListOf(new UserReleaseRoleCreateRequest
            {
                ReleaseId = release.Id,
                ReleaseRole = releaseRole,
            });
            var userPublicationRoles = ListOf(new UserPublicationRoleCreateRequest
            {
                PublicationId = publication.Id,
                PublicationRole = publicationRole,
            });

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);
            emailTemplateService.Setup(mock =>
            mock.SendInviteEmail(
                email.ToLower(),
                It.Is<List<UserReleaseInvite>>(invites =>
                    invites.Count == 1
                    && invites[0].ReleaseVersionId == releaseVersion.Id
                    && invites[0].Role == releaseRole),
                It.Is<List<UserPublicationInvite>>(invites =>
                    invites.Count == 1
                    && invites[0].PublicationId == publication.Id
                    && invites[0].Role == publicationRole)))
                .Returns(Unit.Instance);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email.ToLower(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userReleaseInviteRepository
                .Setup(mock => mock.Create(
                    releaseVersion.Id,
                    email.ToLower(),
                    releaseRole,
                    true,
                    CreatedById,
                    createdDate))
                .Returns(Task.CompletedTask)
                .Callback(async () =>
                {
                    await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

                    contentDbContext.Add(
                        new UserReleaseInvite
                        {
                            Email = email.ToLower(),
                            ReleaseVersionId = releaseVersion.Id,
                            Role = releaseRole,
                            EmailSent = true,
                            Created = createdDate ?? DateTime.UtcNow,
                            CreatedById = CreatedById,
                        }
                    );
                    await contentDbContext.SaveChangesAsync();
                });

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email.ToLower(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userPublicationInviteRepository
                .Setup(mock => mock.CreateManyIfNotExists(
                    userPublicationRoles,
                    email.ToLower(),
                    CreatedById,
                    createdDate))
                .Returns(Task.CompletedTask)
                .Callback(async () =>
                {
                    await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

                    contentDbContext.Add(
                        new UserPublicationInvite
                        {
                            Email = email.ToLower(),
                            PublicationId = publication.Id,
                            Role = publicationRole,
                            Created = createdDate ?? DateTime.UtcNow,
                            CreatedById = CreatedById,
                        }
                    );
                    await contentDbContext.SaveChangesAsync();
                });

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.CreateOrUpdate(
                    email.ToLower(),
                    role.Id,
                    CreatedById,
                    createdDate,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    Email = email.ToLower(),
                    RoleId = role.Id,
                    Active = false,
                    CreatedById = CreatedById,
                    Created = createdDate ?? DateTimeOffset.UtcNow,
                });

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
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userPublicationInviteRepository: userPublicationInviteRepository.Object,
                    userRepository: userRepository.Object);

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = email,
                    RoleId = role.Id,
                    UserReleaseRoles = userReleaseRoles,
                    UserPublicationRoles = userPublicationRoles,
                    CreatedDate = createdDate
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
                emailTemplateService,
                userReleaseInviteRepository,
                userPublicationInviteRepository,
                userRepository);
        }

        [Fact]
        public async Task MultipleReleaseAndPublicationRoles()
        {
            var email = "test@test.com";

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1"
            };

            var publications = _dataFixture.DefaultPublication()
                .ForRange(..2,
                    s => s.SetReleases(
                        [_dataFixture.DefaultRelease(publishedVersions: 1)]))
                .GenerateList(4);

            var release1 = publications[0].Releases.Single();
            var releaseVersion1 = release1.Versions.Single();

            var release2 = publications[1].Releases.Single();
            var releaseVersion2 = release2.Versions.Single();

            var userReleaseRoles = new List<UserReleaseRoleCreateRequest>()
            {
                new() {
                    ReleaseId = release1.Id,
                    ReleaseRole = ReleaseRole.Approver,
                },
                new() {
                    ReleaseId = release2.Id,
                    ReleaseRole = ReleaseRole.Contributor,
                }
            };
            var userPublicationRoles = new List<UserPublicationRoleCreateRequest>()
            {
                new() {
                    PublicationId = publications[2].Id,
                    PublicationRole = PublicationRole.Owner,
                },
                new() {
                    PublicationId = publications[3].Id,
                    PublicationRole = PublicationRole.Allower,
                }
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

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userReleaseInviteRepository
                .Setup(mock => mock.Create(
                    releaseVersion1.Id,
                    email,
                    ReleaseRole.Approver,
                    true,
                    CreatedById,
                    null))
                .Returns(Task.CompletedTask)
                .Callback(async () =>
                {
                    await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

                    contentDbContext.Add(
                     new UserReleaseInvite
                     {
                         Email = email.ToLower(),
                         ReleaseVersionId = releaseVersion1.Id,
                         Role = ReleaseRole.Approver,
                         EmailSent = true,
                         Created = DateTime.UtcNow,
                         CreatedById = CreatedById,
                     }
                );
                    await contentDbContext.SaveChangesAsync();
                });
            userReleaseInviteRepository
                .Setup(mock => mock.Create(
                    releaseVersion2.Id,
                    email,
                    ReleaseRole.Contributor,
                    true,
                    CreatedById,
                    null))
                .Returns(Task.CompletedTask)
                .Callback(async () =>
                {
                    await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

                    contentDbContext.Add(
                         new UserReleaseInvite
                         {
                             Email = email.ToLower(),
                             ReleaseVersionId = releaseVersion2.Id,
                             Role = ReleaseRole.Contributor,
                             EmailSent = true,
                             Created = DateTime.UtcNow,
                             CreatedById = CreatedById,
                         }
                    );
                    await contentDbContext.SaveChangesAsync();
                });

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userPublicationInviteRepository
                .Setup(mock => mock.CreateManyIfNotExists(
                    userPublicationRoles,
                    email,
                    CreatedById,
                    null))
                .Returns(Task.CompletedTask)
                .Callback(async () =>
                {
                    await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

                    contentDbContext.AddRange(
                        new UserPublicationInvite
                        {
                            Email = email.ToLower(),
                            PublicationId = publications[3].Id,
                            Role = PublicationRole.Allower,
                            Created = DateTime.UtcNow,
                            CreatedById = CreatedById,
                        },
                        new UserPublicationInvite
                        {
                            Email = email.ToLower(),
                            PublicationId = publications[2].Id,
                            Role = PublicationRole.Owner,
                            Created = DateTime.UtcNow,
                            CreatedById = CreatedById,
                        }
                    );
                    await contentDbContext.SaveChangesAsync();
                });

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);
            emailTemplateService.Setup(mock =>
                    mock.SendInviteEmail(
                        email,
                        It.Is<List<UserReleaseInvite>>(invites =>
                            invites.Count == 2
                            && invites[0].ReleaseVersionId == releaseVersion1.Id
                            && invites[0].Role == ReleaseRole.Approver
                            && invites[1].ReleaseVersionId == releaseVersion2.Id
                            && invites[1].Role == ReleaseRole.Contributor),
                        It.Is<List<UserPublicationInvite>>(invites =>
                            invites.Count == 2
                            && invites[0].PublicationId == publications[3].Id
                            && invites[0].Role == PublicationRole.Allower
                            && invites[1].PublicationId == publications[2].Id
                            && invites[1].Role == PublicationRole.Owner)))
                .Returns(Unit.Instance);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.CreateOrUpdate(
                    email,
                    role.Id,
                    CreatedById,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    Email = email.ToLower(),
                    RoleId = role.Id,
                    Active = false,
                    CreatedById = CreatedById,
                    Created = DateTimeOffset.UtcNow,
                });

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userPublicationInviteRepository: userPublicationInviteRepository.Object,
                    userRepository: userRepository.Object);

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = email,
                    RoleId = role.Id,
                    UserReleaseRoles = userReleaseRoles,
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
                emailTemplateService,
                userReleaseInviteRepository,
                userPublicationInviteRepository,
                userRepository);
        }

        [Theory]
        [InlineData("test@test.com", "test@test.com")]
        [InlineData("test@test.com", "TEST@test.com")]
        [InlineData("TEST@test.com", "test@test.com")]
        [InlineData("TEST@test.com", "TEST@test.com")]
        public async Task IdentityUserAlreadyExists_ReturnsBadRequest(string identityUserEmailStored, string emailToCheck)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = identityUserEmailStored,
            };

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.Users.Add(user);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserManagementService(usersAndRolesDbContext: userAndRolesDbContext);

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = emailToCheck,
                    RoleId = Guid.NewGuid().ToString(),
                    UserReleaseRoles = [],
                    UserPublicationRoles = []
                };

                var result = await service.InviteUser(inviteRequest);

                result.AssertBadRequest(ValidationErrorMessages.UserAlreadyExists);
            }
        }

        [Fact]
        public async Task InvalidUserRole_ReturnsBadRequest()
        {
            var service = SetupUserManagementService();

            var inviteRequest = new UserInviteCreateRequest
            {
                Email = "test@test.com",
                RoleId = Guid.NewGuid().ToString(),
                UserReleaseRoles = [],
                UserPublicationRoles = []
            };

            var result = await service.InviteUser(inviteRequest);

            result.AssertBadRequest(ValidationErrorMessages.InvalidUserRole);
        }
    }

    public class CancelInviteTests : UserManagementServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var emailToCancelInvitesFor = "test@test.com";
            var otherEmail = "should.not@be.removed";

            var userToCancelInvitesFor = _dataFixture.DefaultUser()
                .WithEmail(emailToCancelInvitesFor)
                .WithCreated(DateTime.UtcNow.AddDays(-1))
                .WithActive(false)
                .Generate();

            var otherUser = _dataFixture.DefaultUser()
                .WithEmail(otherEmail)
                .WithCreated(DateTime.UtcNow.AddDays(-1))
                .WithActive(false)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(userToCancelInvitesFor, otherUser);

                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(emailToCancelInvitesFor, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(emailToCancelInvitesFor, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.SoftDeleteUser(
                    It.Is<User>(u => u.Id == userToCancelInvitesFor.Id), 
                    CreatedById, 
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userPublicationInviteRepository: userPublicationInviteRepository.Object,
                    userRepository: userRepository.Object);

                var result = await service.CancelInvite(emailToCancelInvitesFor);

                result.AssertRight();
            }

            VerifyAllMocks(
                userReleaseInviteRepository,
                userPublicationInviteRepository,
                userRepository);
        }

        [Fact]
        public async Task UserInviteDoesNotExist_ValidationError()
        {
            var service = SetupUserManagementService();

            var result = await service.CancelInvite("test@test.com");

            var actionResult = result.AssertLeft();

            actionResult.AssertValidationProblem(ValidationErrorMessages.InviteNotFound);
        }
    }

    public class DeleteUserTests : UserManagementServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var internalUser = _dataFixture.DefaultUser()
                .Generate();

            var identityUser = new ApplicationUser
            {
                Email = internalUser.Email,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.Users.Add(identityUser);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();
            userManager.Setup(mock => mock.DeleteAsync(
                    It.Is<ApplicationUser>(user => user.Email == internalUser.Email)))
                .ReturnsAsync(new IdentityResult());

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(internalUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(internalUser);
            userRepository
                .Setup(mock => mock.SoftDeleteUser(
                    It.Is<User>(u => u.Id == internalUser.Id),
                    CreatedById,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(internalUser.Email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(internalUser.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
            userPublicationInviteRepository
                .Setup(mock => mock.RemoveByUserEmail(internalUser.Email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(internalUser.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userManager: userManager.Object,
                    userRepository: userRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userPublicationInviteRepository: userPublicationInviteRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object);

                var result = await service.DeleteUser(internalUser.Email);
                result.AssertRight();
            }

            VerifyAllMocks(
                userManager,
                userRepository,
                userReleaseInviteRepository,
                userReleaseRoleRepository,
                userPublicationInviteRepository,
                userPublicationRoleRepository);
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

            var service = SetupUserManagementService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userRepository: userRepository.Object);

            var result = await service.DeleteUser(email);
            result.AssertNotFound();
        }

        [Fact]
        public async Task IdentityUserDoesNotExist_ReturnsNotFound()
        {
            var internalUser = _dataFixture.DefaultUser()
                .Generate();

            await using var usersAndRolesDbContext = InMemoryUserAndRolesDbContext();
            await using var contentDbContext = InMemoryApplicationDbContext();

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(internalUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(internalUser);

            var service = SetupUserManagementService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userRepository: userRepository.Object);

            var result = await service.DeleteUser(internalUser.Email);
            result.AssertNotFound();
        }
    }

    private static UserManagementService SetupUserManagementService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
        IEmailTemplateService? emailTemplateService = null,
        IUserRoleService? userRoleService = null,
        IUserRepository? userRepository = null,
        IUserService? userService = null,
        IUserReleaseInviteRepository? userReleaseInviteRepository = null,
        IUserPublicationInviteRepository? userPublicationInviteRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        UserManager<ApplicationUser>? userManager = null)
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new UserManagementService(
            usersAndRolesDbContext,
            contentDbContext,
            usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
            emailTemplateService ?? Mock.Of<IEmailTemplateService>(Strict),
            userRoleService ?? Mock.Of<IUserRoleService>(Strict),
            userRepository ?? Mock.Of<IUserRepository>(Strict),
            userService ?? AlwaysTrueUserService(CreatedById).Object,
            userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(Strict),
            userPublicationInviteRepository ?? Mock.Of<IUserPublicationInviteRepository>(Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            userManager ?? MockUserManager().Object
        );
    }
}
