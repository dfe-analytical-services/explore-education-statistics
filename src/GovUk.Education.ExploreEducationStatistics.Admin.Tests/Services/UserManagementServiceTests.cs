#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using LinqToDB;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserManagementServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private static readonly Guid CreatedById = Guid.NewGuid();

    public static readonly TheoryData<DateTime?> InviteUserOptionalCreatedDates =
    [
        null,
        DateTime.UtcNow.AddDays(-5)
    ];

    [Fact]
    public async Task GetUser()
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
    public async Task GetUser_NoUser()
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

    [Fact]
    public async Task UpdateUser()
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

    [Theory]
    [MemberData(nameof(InviteUserOptionalCreatedDates))]
    public async Task InviteUser(DateTime? createdDate)
    {
        var existingUserReleaseInvites = new List<UserReleaseInvite>();
        var existingUserPublicationInvites = new List<UserPublicationInvite>();

        var email = "test@test.com";

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

        var emailTemplateService = new Mock<IEmailTemplateService>(Strict);
        emailTemplateService.Setup(mock =>
        mock.SendInviteEmail(
            email,
            It.Is<List<UserReleaseInvite>>(invites =>
                invites.Count == 1
                && invites[0].ReleaseVersionId == releaseVersion.Id
                && invites[0].Role == releaseRole),
            It.Is<List<UserPublicationInvite>>(invites =>
                invites.Count == 1
                && invites[0].PublicationId == publication.Id
                && invites[0].Role == publicationRole)))
            .Returns(Unit.Instance);

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserReleaseInvites);
        userReleaseInviteRepository
            .Setup(mock => mock.RemoveMany(existingUserReleaseInvites, default))
            .Returns(Task.CompletedTask);
        userReleaseInviteRepository
            .Setup(mock => mock.Create(
                releaseVersion.Id,
                email,
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
            })
            .Verifiable();

        var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
        userPublicationInviteRepository
            .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserPublicationInvites);
        userPublicationInviteRepository
            .Setup(mock => mock.RemoveMany(existingUserPublicationInvites, default))
            .Returns(Task.CompletedTask);
        userPublicationInviteRepository
            .Setup(mock => mock.CreateManyIfNotExists(
                userPublicationRoles,
                email,
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
            })
            .Verifiable();

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
                userPublicationInviteRepository: userPublicationInviteRepository.Object);

            var inviteRequest = new UserInviteCreateRequest
            {
                Email = email,
                RoleId = role.Id,
                UserReleaseRoles = userReleaseRoles,
                UserPublicationRoles = userPublicationRoles,
                CreatedDate = createdDate
            };

            var result = await service.InviteUser(inviteRequest);

            result.AssertRight();
        }

        VerifyAllMocks(emailTemplateService, userReleaseInviteRepository, userPublicationInviteRepository);

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = await usersAndRolesDbContext.UserInvites
                .ToListAsync();

            var userInvite = Assert.Single(userInvites);
            Assert.Equal(email, userInvite.Email);
            Assert.False(userInvite.Accepted);
            Assert.Equal(role.Id, userInvite.RoleId);
            userInvite.Created.AssertEqual(createdDate ?? DateTime.UtcNow);
            Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
        }
    }

    [Fact]
    public async Task InviteUser_MultipleReleaseAndPublicationRoles()
    {
        var existingUserReleaseInvites = new List<UserReleaseInvite>();
        var existingUserPublicationInvites = new List<UserPublicationInvite>();

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
            .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserReleaseInvites);
        userReleaseInviteRepository
            .Setup(mock => mock.RemoveMany(existingUserReleaseInvites, default))
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
            })
            .Verifiable();
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
            })
            .Verifiable();

        var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
        userPublicationInviteRepository
            .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserPublicationInvites);
        userPublicationInviteRepository
            .Setup(mock => mock.RemoveMany(existingUserPublicationInvites, default))
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
            })
            .Verifiable();

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

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupUserManagementService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                emailTemplateService: emailTemplateService.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object,
                userPublicationInviteRepository: userPublicationInviteRepository.Object);

            var inviteRequest = new UserInviteCreateRequest
            {
                Email = email,
                RoleId = role.Id,
                UserReleaseRoles = userReleaseRoles,
                UserPublicationRoles = userPublicationRoles,
            };

            var result = await service.InviteUser(inviteRequest);

            result.AssertRight();
        }

        VerifyAllMocks(
            emailTemplateService,
            userReleaseInviteRepository,
            userPublicationInviteRepository);

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = await usersAndRolesDbContext.UserInvites
                .ToListAsync();

            var userInvite = Assert.Single(userInvites);
            Assert.Equal(email, userInvite.Email);
            Assert.False(userInvite.Accepted);
            Assert.Equal(role.Id, userInvite.RoleId);
            userInvite.Created.AssertUtcNow();
            Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
        }
    }

    [Fact]
    public async Task InviteUser_NoUserReleaseRolesAndUserPublicationRoles()
    {
        var existingUserReleaseInvites = new List<UserReleaseInvite>();
        var existingUserPublicationInvites = new List<UserPublicationInvite>();

        var email = "test@test.com";

        var role = new IdentityRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Role 1",
            NormalizedName = "ROLE 1"
        };

        var userPublicationRolesToCreate = new List<UserPublicationRoleCreateRequest>();

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            usersAndRolesDbContext.Roles.Add(role);
            await usersAndRolesDbContext.SaveChangesAsync();
        }

        var emailTemplateService = new Mock<IEmailTemplateService>(Strict);
        emailTemplateService.Setup(mock =>
                mock.SendInviteEmail(
                    email,
                    new List<UserReleaseInvite>(),
                    new List<UserPublicationInvite>()))
            .Returns(Unit.Instance);

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserReleaseInvites);
        userReleaseInviteRepository
            .Setup(mock => mock.RemoveMany(existingUserReleaseInvites, default))
            .Returns(Task.CompletedTask);

        var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
        userPublicationInviteRepository
            .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserPublicationInvites);
        userPublicationInviteRepository
            .Setup(mock => mock.RemoveMany(existingUserPublicationInvites, default))
            .Returns(Task.CompletedTask);
        userPublicationInviteRepository
            .Setup(mock => mock.CreateManyIfNotExists(
                userPublicationRolesToCreate,
                email,
                CreatedById,
                null))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupUserManagementService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                emailTemplateService: emailTemplateService.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object,
                userPublicationInviteRepository: userPublicationInviteRepository.Object);

            var inviteRequest = new UserInviteCreateRequest
            {
                Email = email,
                RoleId = role.Id,
                UserReleaseRoles = [],
                UserPublicationRoles = userPublicationRolesToCreate
            };

            var result = await service.InviteUser(inviteRequest);

            result.AssertRight();
        }

        VerifyAllMocks(
            emailTemplateService,
            userReleaseInviteRepository,
            userPublicationInviteRepository);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = usersAndRolesDbContext.UserInvites
                .ToList();
            var userInvite = Assert.Single(userInvites);
            Assert.Equal(email, userInvite.Email);

            var userReleaseInvites = contentDbContext.UserReleaseInvites
                .ToList();
            Assert.Empty(userReleaseInvites);
        }
    }

    [Fact]
    public async Task InviteUser_UserAlreadyExists()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@test.com",
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
                Email = "test@test.com",
                RoleId = Guid.NewGuid().ToString(),
                UserReleaseRoles = [],
                UserPublicationRoles = []
            };

            var result = await service.InviteUser(inviteRequest);

            result.AssertBadRequest(ValidationErrorMessages.UserAlreadyExists);
        }
    }

    [Fact]
    public async Task InviteUser_UserAlreadyInvited()
    {
        var inviteCreatedDate = DateTime.UtcNow.AddDays(-1);

        await AssertExistingUserInviteOverridden(inviteCreatedDate);
    }

    [Fact]
    public async Task InviteUser_UserAlreadyInvitedAndExpired()
    {
        var inviteCreatedDate = DateTime.UtcNow.AddDays(-15);

        await AssertExistingUserInviteOverridden(inviteCreatedDate);
    }

    private async Task AssertExistingUserInviteOverridden(DateTime inviteCreatedDate)
    {
        var email = "test@test.com";

        var (publication1, publication2) = _dataFixture.DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
            .Generate(2)
            .ToTuple2();

        var release1 = publication1.Releases.Single();
        var releaseVersion1 = release1.Versions.Single();

        var release2 = publication2.Releases.Single();
        var releaseVersion2 = release2.Versions.Single();

        var userReleaseRolesToCreate = new List<UserReleaseRoleCreateRequest>()
        {
            new() {
                ReleaseId = release2.Id,
                ReleaseRole = ReleaseRole.PrereleaseViewer
            }
        };
        var userPublicationRolesToCreate = new List<UserPublicationRoleCreateRequest>()
        {
            new() {
                PublicationId = publication2.Id,
                PublicationRole = PublicationRole.Owner
            }
        };

        var existingUserReleaseInvites = new List<UserReleaseInvite>()
        {
            new() {
                ReleaseVersionId = releaseVersion1.Id,
                Email = email,
                Role = ReleaseRole.Approver,
            },
            new() {
                ReleaseVersionId = releaseVersion1.Id,
                Email = email,
                Role = ReleaseRole.Contributor
            }
        };
        var existingUserPublicationInvites = new List<UserPublicationInvite>()
        {
            new() {
                PublicationId = publication1.Id,
                Email = email,
                Role = PublicationRole.Allower
            }
        };

        var role1 = new IdentityRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Role 1",
            NormalizedName = "ROLE 1"
        };

        var role2 = new IdentityRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Role 2",
            NormalizedName = "ROLE 2"
        };

        var userInvite = new UserInvite
        {
            Email = email,
            RoleId = role1.Id,
            Created = inviteCreatedDate
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            usersAndRolesDbContext.AddRange(role1, role2);
            usersAndRolesDbContext.AddRange(userInvite);
            await usersAndRolesDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.AddRange(publication1, publication2);
            await contentDbContext.SaveChangesAsync();
        }

        var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

        emailTemplateService
            .Setup(mock =>
                mock.SendInviteEmail(
                   email,
                    It.Is<List<UserReleaseInvite>>(invites =>
                        invites.Count == 1
                        && invites[0].ReleaseVersionId == releaseVersion2.Id
                        && invites[0].Role == ReleaseRole.PrereleaseViewer),
                    It.Is<List<UserPublicationInvite>>(invites =>
                        invites.Count == 1
                        && invites[0].PublicationId == publication2.Id
                        && invites[0].Role == PublicationRole.Owner)))
            .Returns(Unit.Instance);

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserReleaseInvites);
        userReleaseInviteRepository
            .Setup(mock => mock.RemoveMany(existingUserReleaseInvites, default))
            .Returns(Task.CompletedTask);
        userReleaseInviteRepository
            .Setup(mock => mock.Create(
                releaseVersion2.Id,
                email,
                ReleaseRole.PrereleaseViewer,
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
                        Role = ReleaseRole.PrereleaseViewer,
                        EmailSent = true,
                        Created = DateTime.UtcNow,
                        CreatedById = CreatedById,
                    }
                );
                await contentDbContext.SaveChangesAsync();
            })
            .Verifiable();

        var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
        userPublicationInviteRepository
            .Setup(mock => mock.GetInvitesByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserPublicationInvites);
        userPublicationInviteRepository
            .Setup(mock => mock.RemoveMany(existingUserPublicationInvites, default))
            .Returns(Task.CompletedTask);
        userPublicationInviteRepository
            .Setup(mock => mock.CreateManyIfNotExists(
                userPublicationRolesToCreate,
                email,
                CreatedById,
                null))
            .Returns(Task.CompletedTask)
            .Callback(async () =>
            {
                await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

                contentDbContext.Add(
                    new UserPublicationInvite
                    {
                        Email = email.ToLower(),
                        PublicationId = publication2.Id,
                        Role = PublicationRole.Owner,
                        Created = DateTime.UtcNow,
                        CreatedById = CreatedById,
                    }
                );
                await contentDbContext.SaveChangesAsync();
            })
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupUserManagementService(
                emailTemplateService: emailTemplateService.Object,
                usersAndRolesDbContext: usersAndRolesDbContext,
                contentDbContext: contentDbContext,
                userReleaseInviteRepository: userReleaseInviteRepository.Object,
                userPublicationInviteRepository: userPublicationInviteRepository.Object);

            var inviteRequest = new UserInviteCreateRequest
            {
                Email = email,
                RoleId = role2.Id,
                UserReleaseRoles = userReleaseRolesToCreate,
                UserPublicationRoles = userPublicationRolesToCreate
            };

            var result = await service.InviteUser(inviteRequest);

            result.AssertRight();
        }

        VerifyAllMocks(
            emailTemplateService,
            userReleaseInviteRepository,
            userPublicationInviteRepository);

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = await usersAndRolesDbContext.UserInvites
                .ToListAsync();

            var updatedUserInvite = Assert.Single(userInvites);
            Assert.Equal(email, updatedUserInvite.Email);
            Assert.False(updatedUserInvite.Accepted);
            Assert.Equal(role2.Id, updatedUserInvite.RoleId);
            updatedUserInvite.Created.AssertUtcNow();
            Assert.Equal(CreatedById.ToString(), updatedUserInvite.CreatedById);
        }
    }

    [Fact]
    public async Task InviteUser_InvalidUserRole()
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

    [Fact]
    public async Task CancelInvite()
    {
        var emailToCancelInvitesFor = "test@test.com";
        var otherEmail = "should.not@be.removed";

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            usersAndRolesDbContext.UserInvites.AddRange(
            new UserInvite
            {
                Email = emailToCancelInvitesFor,
                Created = DateTime.UtcNow.AddDays(-1)
            },
            new UserInvite
            {
                Email = otherEmail,
                Created = DateTime.UtcNow.AddDays(-1)
            });
            await usersAndRolesDbContext.SaveChangesAsync();
        }

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.RemoveByUserEmail(emailToCancelInvitesFor, default))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
        userPublicationInviteRepository
            .Setup(mock => mock.RemoveByUserEmail(emailToCancelInvitesFor, default))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupUserManagementService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userReleaseInviteRepository: userReleaseInviteRepository.Object,
                userPublicationInviteRepository: userPublicationInviteRepository.Object);

            var result = await service.CancelInvite(emailToCancelInvitesFor);

            result.AssertRight();
        }

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var invites = usersAndRolesDbContext.UserInvites.ToList();
            var unremovedInvite = Assert.Single(invites);
            Assert.Equal(otherEmail, unremovedInvite.Email);
        }

        VerifyAllMocks(userReleaseInviteRepository, userPublicationInviteRepository);
    }

    [Fact]
    public async Task CancelInvite_InviteNotFound()
    {
        var service = SetupUserManagementService();
        var result = await service.CancelInvite("test@test.com");
        var actionResult = result.AssertLeft();
        actionResult.AssertValidationProblem(ValidationErrorMessages.InviteNotFound);
    }

    [Fact]
    public async Task DeleteUser()
    {
        var internalUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
        };

        var identityUser = new ApplicationUser
        {
            Email = internalUser.Email,
        };

        var invite = new UserInvite
        {
            Email = internalUser.Email,
        };

        var releaseInvite = _dataFixture.DefaultUserReleaseInvite()
            .WithEmail(internalUser.Email)
            .Generate();

        var publicationInvite = _dataFixture.DefaultUserPublicationInvite()
            .WithEmail(internalUser.Email)
            .Generate();

        var releaseRole = _dataFixture.DefaultUserReleaseRole()
            .WithUser(internalUser)
            .Generate();

        var publicationRole = _dataFixture.DefaultUserPublicationRole()
            .WithUser(internalUser)
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserReleaseInvites.Add(releaseInvite);
            contentDbContext.UserPublicationInvites.Add(publicationInvite);
            contentDbContext.UserReleaseRoles.Add(releaseRole);
            contentDbContext.UserPublicationRoles.Add(publicationRole);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            usersAndRolesDbContext.Users.Add(identityUser);
            usersAndRolesDbContext.UserInvites.Add(invite);
            await usersAndRolesDbContext.SaveChangesAsync();
        }

        var userManager = MockUserManager();
        userManager.Setup(mock => mock.DeleteAsync(
                It.Is<ApplicationUser>(user => user.Email == internalUser.Email)))
            .ReturnsAsync(new IdentityResult())
            .Verifiable();

        var userRepository = new Mock<IUserRepository>(Strict);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
        userReleaseRoleRepository
            .Setup(mock => mock.RemoveForUser(internalUser.Id, default))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
        userPublicationRoleRepository
            .Setup(mock => mock.RemoveForUser(internalUser.Id, default))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            userRepository
                .Setup(mock => mock.FindByEmail(internalUser.Email))
                .ReturnsAsync(() => contentDbContext.Users.Single(u => u.Email == internalUser.Email))
                .Verifiable();

            var service = SetupUserManagementService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userManager: userManager.Object,
                userRepository: userRepository.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object);

            var result = await service.DeleteUser(internalUser.Email);
            result.AssertRight();
        }

        // Mock checks that ApplicationUser has been removed, so no check required here

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var invites = await usersAndRolesDbContext.UserInvites.ToListAsync();
            Assert.Empty(invites);

            var dbInternalUser = await contentDbContext.Users.SingleAsync();
            Assert.Equal(internalUser.Email, dbInternalUser.Email);
            Assert.NotNull(dbInternalUser.SoftDeleted);
            dbInternalUser.SoftDeleted.AssertUtcNow();
            Assert.Equal(CreatedById, dbInternalUser.DeletedById);

            VerifyAllMocks(
                userManager,
                userRepository,
                userReleaseRoleRepository,
                userPublicationRoleRepository);
        }
    }

    [Fact]
    public async Task DeleteUser_NotFound()
    {
        var email = "test@test.com";

        await using var usersAndRolesDbContext = InMemoryUserAndRolesDbContext();
        await using var contentDbContext = InMemoryApplicationDbContext();

        var userRepository = new Mock<IUserRepository>(Strict);
        userRepository
            .Setup(mock => mock.FindByEmail(email))
            .ReturnsAsync((User?)null)
            .Verifiable();

        var service = SetupUserManagementService(
            contentDbContext: contentDbContext,
            usersAndRolesDbContext: usersAndRolesDbContext,
            userRepository: userRepository.Object);

        var result = await service.DeleteUser(email);
        result.AssertNotFound();
    }

    private static UserManagementService SetupUserManagementService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
        IEmailTemplateService? emailTemplateService = null,
        IUserRoleService? userRoleService = null,
        IUserRepository? userRepository = null,
        IUserService? userService = null,
        IUserInviteRepository? userInviteRepository = null,
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
            userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
            userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(Strict),
            userPublicationInviteRepository ?? Mock.Of<IUserPublicationInviteRepository>(Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            userManager ?? MockUserManager().Object
        );
    }
}
