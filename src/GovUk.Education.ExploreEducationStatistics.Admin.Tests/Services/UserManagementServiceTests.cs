#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Microsoft.AspNetCore.Identity;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserManagementServiceTests
    {
        private readonly DataFixture _dataFixture = new();

        private static readonly Guid CreatedById = Guid.NewGuid();

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

        [Fact]
        public async Task InviteUser()
        {
            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1"
            };

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.Roles.Add(role);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendInviteEmail(
                        "test@test.com",
                        It.Is<List<UserReleaseInvite>>(invites =>
                            invites.Count == 1
                            && invites[0].ReleaseVersionId == releaseVersion.Id
                            && invites[0].Role == ReleaseRole.Approver),
                        It.Is<List<UserPublicationInvite>>(invites =>
                            invites.Count == 1
                            && invites[0].PublicationId == publication.Id
                            && invites[0].Role == PublicationRole.Owner)))
                .Returns(Unit.Instance);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    emailTemplateService: emailTemplateService.Object);

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = "test@test.com",
                    RoleId = role.Id,
                    UserReleaseRoles = ListOf(new UserReleaseRoleCreateRequest
                    {
                        ReleaseId = release.Id,
                        ReleaseRole = ReleaseRole.Approver,
                    }),
                    UserPublicationRoles = ListOf(new UserPublicationRoleCreateRequest
                    {
                        PublicationId = publication.Id,
                        PublicationRole = PublicationRole.Owner,
                    })
                };

                var result = await service.InviteUser(inviteRequest);

                VerifyAllMocks(emailTemplateService);

                result.AssertRight();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = usersAndRolesDbContext.UserInvites
                    .ToList();

                var userInvite = Assert.Single(userInvites);
                Assert.Equal("test@test.com", userInvite.Email);
                Assert.False(userInvite.Accepted);
                Assert.Equal(role.Id, userInvite.RoleId);
                userInvite.Created.AssertUtcNow();
                Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = contentDbContext.UserReleaseInvites
                    .ToList();
                var userReleaseInvite = Assert.Single(userReleaseInvites);
                Assert.Equal("test@test.com", userReleaseInvite.Email);
                Assert.Equal(releaseVersion.Id, userReleaseInvite.ReleaseVersionId);
                Assert.Equal(ReleaseRole.Approver, userReleaseInvite.Role);
                Assert.True(userReleaseInvite.EmailSent);
                userReleaseInvite.Created.AssertUtcNow();
                Assert.Equal(CreatedById, userReleaseInvite.CreatedById);

                var userPublicationInvites = contentDbContext.UserPublicationInvites
                    .ToList();
                var userPublicationInvite = Assert.Single(userPublicationInvites);
                Assert.Equal("test@test.com", userPublicationInvite.Email);
                Assert.Equal(publication.Id, userPublicationInvite.PublicationId);
                Assert.Equal(PublicationRole.Owner, userPublicationInvite.Role);
                userPublicationInvite.Created.AssertUtcNow();
                Assert.Equal(CreatedById, userPublicationInvite.CreatedById);
            }
        }

        [Fact]
        public async Task InviteUser_OptionalCreatedDate()
        {
            // We are passing in an optional CreatedDate
            var createdDate = DateTime.UtcNow.AddDays(-5);

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1"
            };

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await usersAndRolesDbContext.AddRangeAsync(role);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendInviteEmail(
                        "test@test.com",
                        It.Is<List<UserReleaseInvite>>(invites =>
                            invites.Count == 1
                            && invites[0].ReleaseVersionId == releaseVersion.Id
                            && invites[0].Role == ReleaseRole.Approver),
                        It.Is<List<UserPublicationInvite>>(invites =>
                            invites.Count == 1
                            && invites[0].PublicationId == publication.Id
                            && invites[0].Role == PublicationRole.Owner)))
                .Returns(Unit.Instance);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    emailTemplateService: emailTemplateService.Object);

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = "test@test.com",
                    RoleId = role.Id,
                    UserReleaseRoles = ListOf(new UserReleaseRoleCreateRequest
                    {
                        ReleaseId = release.Id,
                        ReleaseRole = ReleaseRole.Approver,
                    }),
                    UserPublicationRoles = ListOf(new UserPublicationRoleCreateRequest
                    {
                        PublicationId = publication.Id,
                        PublicationRole = PublicationRole.Owner,
                    }),
                    CreatedDate = createdDate
                };

                var result = await service.InviteUser(inviteRequest);

                VerifyAllMocks(emailTemplateService);

                result.AssertRight();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = usersAndRolesDbContext.UserInvites
                    .ToList();

                var userInvite = Assert.Single(userInvites);
                Assert.Equal("test@test.com", userInvite.Email);
                Assert.False(userInvite.Accepted);
                Assert.Equal(role.Id, userInvite.RoleId);
                Assert.Equal(createdDate, userInvite.Created);
                Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = contentDbContext.UserReleaseInvites
                    .ToList();
                var userReleaseInvite = Assert.Single(userReleaseInvites);
                Assert.Equal("test@test.com", userReleaseInvite.Email);
                Assert.Equal(releaseVersion.Id, userReleaseInvite.ReleaseVersionId);
                Assert.Equal(ReleaseRole.Approver, userReleaseInvite.Role);
                Assert.True(userReleaseInvite.EmailSent);
                Assert.Equal(createdDate, userReleaseInvite.Created);
                Assert.Equal(CreatedById, userReleaseInvite.CreatedById);

                var userPublicationInvites = contentDbContext.UserPublicationInvites
                    .ToList();
                var userPublicationInvite = Assert.Single(userPublicationInvites);
                Assert.Equal("test@test.com", userPublicationInvite.Email);
                Assert.Equal(publication.Id, userPublicationInvite.PublicationId);
                Assert.Equal(PublicationRole.Owner, userPublicationInvite.Role);
                Assert.Equal(createdDate, userPublicationInvite.Created);
                Assert.Equal(CreatedById, userPublicationInvite.CreatedById);
            }
        }

        [Fact]
        public async Task InviteUser_MultipleReleaseAndPublicationRoles()
        {
            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1"
            };

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.Roles.Add(role);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var publications = _dataFixture.DefaultPublication()
                .ForRange(..2,
                    s => s.SetReleases(
                        [_dataFixture.DefaultRelease(publishedVersions: 1)]))
                .GenerateList(4);

            var release1 = publications[0].Releases.Single();
            var releaseVersion1 = release1.Versions.Single();

            var release2 = publications[1].Releases.Single();
            var releaseVersion2 = release2.Versions.Single();

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendInviteEmail(
                        "test@test.com",
                        It.Is<List<UserReleaseInvite>>(invites =>
                            invites.Count == 2
                            && invites[0].ReleaseVersionId == releaseVersion1.Id
                            && invites[0].Role == ReleaseRole.Approver
                            && invites[1].ReleaseVersionId == releaseVersion2.Id
                            && invites[1].Role == ReleaseRole.Contributor),
                        It.Is<List<UserPublicationInvite>>(invites =>
                            invites.Count == 2
                            && invites[0].PublicationId == publications[2].Id
                            && invites[0].Role == PublicationRole.Owner
                            && invites[1].PublicationId == publications[3].Id
                            && invites[1].Role == PublicationRole.Allower)))
                .Returns(Unit.Instance);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publications);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    emailTemplateService: emailTemplateService.Object);

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = "test@test.com",
                    RoleId = role.Id,
                    UserReleaseRoles =
                    [
                        new UserReleaseRoleCreateRequest
                        {
                            ReleaseId = release1.Id,
                            ReleaseRole = ReleaseRole.Approver,
                        },
                        new UserReleaseRoleCreateRequest
                        {
                            ReleaseId = release2.Id,
                            ReleaseRole = ReleaseRole.Contributor,
                        }
                    ],
                    UserPublicationRoles =
                    [
                        new UserPublicationRoleCreateRequest
                        {
                            PublicationId = publications[2].Id,
                            PublicationRole = PublicationRole.Owner,
                        },
                        new UserPublicationRoleCreateRequest
                        {
                            PublicationId = publications[3].Id,
                            PublicationRole = PublicationRole.Allower,
                        }
                    ]
                };

                var result = await service.InviteUser(inviteRequest);

                VerifyAllMocks(emailTemplateService);

                result.AssertRight();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = usersAndRolesDbContext.UserInvites
                    .ToList();

                var userInvite = Assert.Single(userInvites);
                Assert.Equal("test@test.com", userInvite.Email);
                Assert.False(userInvite.Accepted);
                Assert.Equal(role.Id, userInvite.RoleId);
                userInvite.Created.AssertUtcNow();
                Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = contentDbContext.UserReleaseInvites
                    .ToList();

                Assert.Equal(2, userReleaseInvites.Count);

                Assert.Equal("test@test.com", userReleaseInvites[0].Email);
                Assert.Equal(releaseVersion1.Id, userReleaseInvites[0].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Approver, userReleaseInvites[0].Role);
                Assert.True(userReleaseInvites[0].EmailSent);
                userReleaseInvites[0].Created.AssertUtcNow();
                Assert.Equal(CreatedById, userReleaseInvites[0].CreatedById);

                Assert.Equal("test@test.com", userReleaseInvites[1].Email);
                Assert.Equal(releaseVersion2.Id, userReleaseInvites[1].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseInvites[1].Role);
                Assert.True(userReleaseInvites[1].EmailSent);
                userReleaseInvites[1].Created.AssertUtcNow();
                Assert.Equal(CreatedById, userReleaseInvites[1].CreatedById);

                var userPublicationInvites = contentDbContext.UserPublicationInvites
                    .ToList();
                Assert.Equal(2, userPublicationInvites.Count);

                Assert.Equal("test@test.com", userPublicationInvites[0].Email);
                Assert.Equal(publications[2].Id, userPublicationInvites[0].PublicationId);
                Assert.Equal(PublicationRole.Owner, userPublicationInvites[0].Role);
                userPublicationInvites[0].Created.AssertUtcNow();
                Assert.Equal(CreatedById, userPublicationInvites[0].CreatedById);

                Assert.Equal("test@test.com", userPublicationInvites[1].Email);
                Assert.Equal(publications[3].Id, userPublicationInvites[1].PublicationId);
                Assert.Equal(PublicationRole.Allower, userPublicationInvites[1].Role);
                userPublicationInvites[1].Created.AssertUtcNow();
                Assert.Equal(CreatedById, userPublicationInvites[1].CreatedById);
            }
        }

        [Fact]
        public async Task InviteUser_NoUserReleaseRolesAndUserPublicationRoles()
        {
            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1"
            };

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                usersAndRolesDbContext.Roles.Add(role);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendInviteEmail(
                        "test@test.com",
                        new List<UserReleaseInvite>(),
                        new List<UserPublicationInvite>()))
                .Returns(Unit.Instance);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    emailTemplateService: emailTemplateService.Object);

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = "test@test.com",
                    RoleId = role.Id,
                    UserReleaseRoles = [],
                    UserPublicationRoles = []
                };

                var result = await service.InviteUser(inviteRequest);

                VerifyAllMocks(emailTemplateService);

                result.AssertRight();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = usersAndRolesDbContext.UserInvites
                    .ToList();
                var userInvite = Assert.Single(userInvites);
                Assert.Equal("test@test.com", userInvite.Email);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
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
            var (publication1, publication2) = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                .Generate(2)
                .ToTuple2();

            var release1 = publication1.Releases.Single();
            var releaseVersion1 = release1.Versions.Single();

            var release2 = publication2.Releases.Single();
            var releaseVersion2 = release2.Versions.Single();

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

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await usersAndRolesDbContext.AddRangeAsync(role1, role2);

                var userInvite = new UserInvite
                {
                    Email = "test@test.com",
                    RoleId = role1.Id,
                    Created = inviteCreatedDate
                };
                await usersAndRolesDbContext.AddRangeAsync(userInvite);

                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publication1, publication2);

                contentDbContext.UserReleaseInvites.AddRange(
                    new UserReleaseInvite
                    {
                        ReleaseVersionId = releaseVersion1.Id,
                        Email = "test@test.com",
                        Role = ReleaseRole.Approver,
                    },
                    new UserReleaseInvite
                    {
                        ReleaseVersionId = releaseVersion1.Id,
                        Email = "test@test.com",
                        Role = ReleaseRole.Contributor
                    });
                contentDbContext.UserPublicationInvites.AddRange(
                    new UserPublicationInvite
                    {
                        PublicationId = publication1.Id,
                        Email = "test@test.com",
                        Role = PublicationRole.Allower
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService
                .Setup(mock =>
                    mock.SendInviteEmail(
                        "test@test.com",
                        It.Is<List<UserReleaseInvite>>(invites =>
                            invites.Count == 1
                            && invites[0].ReleaseVersionId == releaseVersion2.Id
                            && invites[0].Role == ReleaseRole.PrereleaseViewer),
                        It.Is<List<UserPublicationInvite>>(invites =>
                            invites.Count == 1
                            && invites[0].PublicationId == publication2.Id
                            && invites[0].Role == PublicationRole.Owner)))
                .Returns(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    emailTemplateService: emailTemplateService.Object,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext);

                var inviteRequest = new UserInviteCreateRequest
                {
                    Email = "test@test.com",
                    RoleId = role2.Id,
                    UserReleaseRoles =
                    [
                        new UserReleaseRoleCreateRequest
                        {
                            ReleaseId = release2.Id,
                            ReleaseRole = ReleaseRole.PrereleaseViewer
                        }
                    ],
                    UserPublicationRoles =
                    [
                        new UserPublicationRoleCreateRequest
                        {
                            PublicationId = publication2.Id,
                            PublicationRole = PublicationRole.Owner
                        }
                    ]
                };

                var result = await service.InviteUser(inviteRequest);

                VerifyAllMocks(emailTemplateService);

                result.AssertRight();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = usersAndRolesDbContext.UserInvites
                    .ToList();

                var userInvite = Assert.Single(userInvites);
                Assert.Equal("test@test.com", userInvite.Email);
                Assert.False(userInvite.Accepted);
                Assert.Equal(role2.Id, userInvite.RoleId);
                userInvite.Created.AssertUtcNow();
                Assert.Equal(CreatedById.ToString(), userInvite.CreatedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = contentDbContext.UserReleaseInvites
                    .ToList();
                var userReleaseInvite = Assert.Single(userReleaseInvites);
                Assert.Equal("test@test.com", userReleaseInvite.Email);
                Assert.Equal(releaseVersion2.Id, userReleaseInvite.ReleaseVersionId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, userReleaseInvite.Role);
                Assert.True(userReleaseInvite.EmailSent);
                userReleaseInvite.Created.AssertUtcNow();
                Assert.Equal(CreatedById, userReleaseInvite.CreatedById);

                var userPublicationInvites = contentDbContext.UserPublicationInvites
                    .ToList();
                var userPublicationInvite = Assert.Single(userPublicationInvites);
                Assert.Equal("test@test.com", userPublicationInvite.Email);
                Assert.Equal(publication2.Id, userPublicationInvite.PublicationId);
                Assert.Equal(PublicationRole.Owner, userPublicationInvite.Role);
                userPublicationInvite.Created.AssertUtcNow();
                Assert.Equal(CreatedById, userPublicationInvite.CreatedById);
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
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await usersAndRolesDbContext.UserInvites.AddRangeAsync(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        Created = DateTime.UtcNow.AddDays(-1)
                    },
                    new UserInvite
                    {
                        Email = "should.not@be.removed",
                        Created = DateTime.UtcNow.AddDays(-1)
                    });
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseInvites.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        Email = "test@test.com",
                        Role = ReleaseRole.Approver,
                        Created = DateTime.UtcNow.AddDays(-1)
                    },
                    new UserReleaseInvite
                    {
                        Email = "test@test.com",
                        Role = ReleaseRole.Contributor
                    },
                    new UserReleaseInvite
                    {
                        Email = "should.not@be.removed",
                        Role = ReleaseRole.Approver
                    });
                await contentDbContext.UserPublicationInvites.AddRangeAsync(
                    new UserPublicationInvite
                    {
                        Email = "test@test.com",
                        Role = PublicationRole.Owner
                    },
                    new UserPublicationInvite
                    {
                        Email = "test@test.com",
                        Role = PublicationRole.Allower
                    },
                    new UserPublicationInvite
                    {
                        Email = "should.not@be.removed",
                        Role = PublicationRole.Owner
                    });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext);

                var result = await service.CancelInvite("test@test.com");

                result.AssertRight();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var invites = usersAndRolesDbContext.UserInvites.ToList();
                var unremovedInvite = Assert.Single(invites);
                Assert.Equal("should.not@be.removed", unremovedInvite.Email);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseInvites = contentDbContext.UserReleaseInvites.ToList();
                var unremovedReleaseInvite = Assert.Single(releaseInvites);
                Assert.Equal("should.not@be.removed", unremovedReleaseInvite.Email);

                var publicationInvites = contentDbContext.UserPublicationInvites.ToList();
                var unremovedPublicationInvite = Assert.Single(publicationInvites);
                Assert.Equal("should.not@be.removed", unremovedPublicationInvite.Email);
            }
        }

        [Fact]
        public async Task CancelInvite_NoUserReleaseRoles()
        {
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await usersAndRolesDbContext.UserInvites.AddRangeAsync(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        Created = DateTime.UtcNow.AddDays(-1)
                    });
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    usersAndRolesDbContext: usersAndRolesDbContext);
                var result = await service.CancelInvite("test@test.com");
                result.AssertRight();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var invites = usersAndRolesDbContext.UserInvites.ToList();
                Assert.Empty(invites);
            }
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
            var userId = Guid.NewGuid();
            var email = "test@test.com";

            var invite = new UserInvite
            {
                Email = email,
            };

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                // Not adding identity user, as it is deleted via userManager.DeleteAsync
                usersAndRolesDbContext.UserInvites.Add(invite);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var internalUser = new User
            {
                Id = userId,
                Email = email,
            };

            var releaseInvite = new UserReleaseInvite
            {
                Email = email,
            };

            var publicationInvite = new UserPublicationInvite
            {
                Email = email,
            };

            var releaseRole = new UserReleaseRole
            {
                UserId = userId,
            };

            var publicationRole = new UserPublicationRole
            {
                UserId = userId,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(internalUser);
                contentDbContext.UserReleaseInvites.Add(releaseInvite);
                contentDbContext.UserPublicationInvites.Add(publicationInvite);
                contentDbContext.UserReleaseRoles.Add(releaseRole);
                contentDbContext.UserPublicationRoles.Add(publicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userManager = MockUserManager();
                userManager.Setup(mock => mock.DeleteAsync(
                        It.Is<ApplicationUser>(user => user.Email == email)))
                    .ReturnsAsync(new IdentityResult());

                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.DeleteUser(email);
                result.AssertRight();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                // Mock checks that ApplicationUser has been removed, so no check required here

                var invites = usersAndRolesDbContext.UserInvites.ToList();
                Assert.Empty(invites);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseInvites = contentDbContext.UserReleaseInvites.ToList();
                Assert.Empty(releaseInvites);

                var publicationInvites = contentDbContext.UserPublicationInvites.ToList();
                Assert.Empty(publicationInvites);

                var releaseRoles = contentDbContext.UserReleaseRoles.ToList();
                Assert.Empty(releaseRoles);

                var publicationRoles = contentDbContext.UserPublicationRoles.ToList();
                Assert.Empty(publicationRoles);

                var dbInternalUserList = contentDbContext.Users.ToList();
                var dbInternalUser = Assert.Single(dbInternalUserList);
                Assert.Equal(email, dbInternalUser.Email);
                Assert.NotNull(dbInternalUser.SoftDeleted);
                dbInternalUser.SoftDeleted.AssertUtcNow();
                Assert.Equal(CreatedById, dbInternalUser.DeletedById);
            }
        }

        [Fact]
        public async Task DeleteUser_NotFound()
        {
            var email = "test@test.com";

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId);
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var service = SetupUserManagementService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext);

            var result = await service.DeleteUser(email);
            result.AssertNotFound();
        }

        private static UserManagementService SetupUserManagementService(
            ContentDbContext? contentDbContext = null,
            UsersAndRolesDbContext? usersAndRolesDbContext = null,
            IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
            IEmailTemplateService? emailTemplateService = null,
            IUserRoleService? userRoleService = null,
            IUserService? userService = null,
            IUserInviteRepository? userInviteRepository = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null,
            IUserPublicationInviteRepository? userPublicationInviteRepository = null,
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
                userService ?? AlwaysTrueUserService(CreatedById).Object,
                userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
                userReleaseInviteRepository ?? new UserReleaseInviteRepository(contentDbContext),
                userPublicationInviteRepository ?? new UserPublicationInviteRepository(contentDbContext),
                userManager ?? MockUserManager().Object
            );
        }
    }
}
