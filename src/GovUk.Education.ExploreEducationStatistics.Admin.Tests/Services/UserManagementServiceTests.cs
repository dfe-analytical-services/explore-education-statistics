#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserManagementServiceTests
    {
        private static readonly Guid _createdById = Guid.NewGuid();

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

            var globalRoles = new List<RoleViewModel>
            {
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
                }
            };

            var publicationRoles = new List<UserPublicationRoleViewModel>
            {
                new UserPublicationRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 1",
                    Role = Owner
                },
                new UserPublicationRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 2",
                    Role = Owner
                }
            };

            var releaseRoles = new List<UserReleaseRoleViewModel>
            {
                new UserReleaseRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 1",
                    Release = "December 2020",
                    Role = Contributor
                },
                new UserReleaseRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 2",
                    Release = "June 2021",
                    Role = ReleaseRole.Approver
                }
            };

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
                mock.GetPublicationRoles(userId)).ReturnsAsync(publicationRoles);

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
                userRoleService.Verify(mock => mock.GetPublicationRoles(userId), Times.Once);
                userRoleService.Verify(mock => mock.GetReleaseRoles(userId), Times.Once);
            }
        }

        [Fact]
        public async Task GetUser_NoUser()
        {
            var userRoleService = new Mock<IUserRoleService>(Strict);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupUserManagementService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userRoleService: userRoleService.Object);

                var result = await service.GetUser(Guid.NewGuid());

                VerifyAllMocks(userRoleService);

                result.AssertNotFound();
            }
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
                await usersAndRolesDbContext.AddRangeAsync(role);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var release = new Release
            {
                Publication = new Publication(),
            };
            var publication = new Publication();

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendInviteEmail(
                        "test@test.com",
                        It.Is<List<UserReleaseInvite>>(invites =>
                            invites.Count == 1
                            && invites[0].ReleaseId == release.Id
                            && invites[0].Role == ReleaseRole.Approver),
                        It.Is<List<UserPublicationInvite>>(invites =>
                            invites.Count == 1
                            && invites[0].PublicationId == publication.Id
                            && invites[0].Role == Owner)))
                .Returns(Unit.Instance);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.Publications.AddRangeAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    emailTemplateService: emailTemplateService.Object);

                var result = await service.InviteUser("test@test.com",
                    role.Id,
                    new List<UserReleaseRoleAddViewModel>
                    {
                        new ()
                        {
                            ReleaseId = release.Id,
                            ReleaseRole = ReleaseRole.Approver,
                        },
                    },
                    new List<UserPublicationRoleAddViewModel>
                    {
                        new ()
                        {
                          PublicationId = publication.Id,
                          PublicationRole = Owner,
                        },
                    });

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
                Assert.InRange(DateTime.UtcNow.Subtract(userInvite.Created).Milliseconds, 0, 1500);
                Assert.Equal(_createdById.ToString(), userInvite.CreatedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = contentDbContext.UserReleaseInvites
                    .ToList();
                var userReleaseInvite = Assert.Single(userReleaseInvites);
                Assert.Equal("test@test.com", userReleaseInvite.Email);
                Assert.Equal(release.Id, userReleaseInvite.ReleaseId);
                Assert.Equal(ReleaseRole.Approver, userReleaseInvite.Role);
                Assert.True(userReleaseInvite.EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvite.Created).Milliseconds, 0, 1500);
                Assert.Equal(_createdById, userReleaseInvite.CreatedById);

                var userPublicationInvites = contentDbContext.UserPublicationInvites
                    .ToList();
                var userPublicationInvite = Assert.Single(userPublicationInvites);
                Assert.Equal("test@test.com", userPublicationInvite.Email);
                Assert.Equal(publication.Id, userPublicationInvite.PublicationId);
                Assert.Equal(Owner, userPublicationInvite.Role);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationInvite.Created).Milliseconds, 0, 1500);
                Assert.Equal(_createdById, userPublicationInvite.CreatedById);
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
                await usersAndRolesDbContext.AddRangeAsync(role);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var release1 = new Release
            {
                Publication = new Publication(),
            };
            var release2 = new Release
            {
                Publication = new Publication(),
            };
            var publication1 = new Publication();
            var publication2 = new Publication();

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendInviteEmail(
                        "test@test.com",
                        It.Is<List<UserReleaseInvite>>(invites =>
                            invites.Count == 2
                            && invites[0].ReleaseId == release1.Id
                            && invites[0].Role == ReleaseRole.Approver
                            && invites[1].ReleaseId == release2.Id
                            && invites[1].Role == Contributor),
                        It.Is<List<UserPublicationInvite>>(invites =>
                            invites.Count == 2
                            && invites[0].PublicationId == publication1.Id
                            && invites[0].Role == Owner
                            && invites[1].PublicationId == publication2.Id
                            && invites[1].Role == PublicationRole.Approver)))
                .Returns(Unit.Instance);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release1, release2);
                await contentDbContext.Publications.AddRangeAsync(publication1, publication2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    emailTemplateService: emailTemplateService.Object);

                var result = await service.InviteUser("test@test.com",
                    role.Id,
                    new List<UserReleaseRoleAddViewModel>
                    {
                        new ()
                        {
                            ReleaseId = release1.Id,
                            ReleaseRole = ReleaseRole.Approver,
                        },
                        new ()
                        {
                            ReleaseId = release2.Id,
                            ReleaseRole = Contributor,
                        }
                    },
                    new List<UserPublicationRoleAddViewModel>
                    {
                        new ()
                        {
                          PublicationId = publication1.Id,
                          PublicationRole = Owner,
                        },
                        new ()
                        {
                          PublicationId = publication2.Id,
                          PublicationRole = PublicationRole.Approver,
                        },
                    });

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
                Assert.InRange(DateTime.UtcNow.Subtract(userInvite.Created).Milliseconds, 0, 1500);
                Assert.Equal(_createdById.ToString(), userInvite.CreatedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = contentDbContext.UserReleaseInvites
                    .ToList();

                Assert.Equal(2, userReleaseInvites.Count);

                Assert.Equal("test@test.com", userReleaseInvites[0].Email);
                Assert.Equal(release1.Id, userReleaseInvites[0].ReleaseId);
                Assert.Equal(ReleaseRole.Approver, userReleaseInvites[0].Role);
                Assert.True(userReleaseInvites[0].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[0].Created).Milliseconds, 0, 1500);
                Assert.Equal(_createdById, userReleaseInvites[0].CreatedById);

                Assert.Equal("test@test.com", userReleaseInvites[1].Email);
                Assert.Equal(release2.Id, userReleaseInvites[1].ReleaseId);
                Assert.Equal(Contributor, userReleaseInvites[1].Role);
                Assert.True(userReleaseInvites[1].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[1].Created).Milliseconds, 0, 1500);
                Assert.Equal(_createdById, userReleaseInvites[1].CreatedById);

                var userPublicationInvites = contentDbContext.UserPublicationInvites
                    .ToList();
                Assert.Equal(2, userPublicationInvites.Count);

                Assert.Equal("test@test.com", userPublicationInvites[0].Email);
                Assert.Equal(publication1.Id, userPublicationInvites[0].PublicationId);
                Assert.Equal(Owner, userPublicationInvites[0].Role);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationInvites[0].Created).Milliseconds, 0, 1500);
                Assert.Equal(_createdById, userPublicationInvites[0].CreatedById);

                Assert.Equal("test@test.com", userPublicationInvites[1].Email);
                Assert.Equal(publication2.Id, userPublicationInvites[1].PublicationId);
                Assert.Equal(PublicationRole.Approver, userPublicationInvites[1].Role);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationInvites[1].Created).Milliseconds, 0, 1500);
                Assert.Equal(_createdById, userPublicationInvites[1].CreatedById);
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
                await usersAndRolesDbContext.AddRangeAsync(role);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(new Release());
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendInviteEmail(
                        "test@test.com",
                        new List<UserReleaseInvite>(),
                        new List<UserPublicationInvite>()))
                .Returns(Unit.Instance);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    emailTemplateService: emailTemplateService.Object);

                var result = await service.InviteUser("test@test.com",
                    role.Id,
                    new List<UserReleaseRoleAddViewModel>(),
                    new List<UserPublicationRoleAddViewModel>());

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
                await usersAndRolesDbContext.AddRangeAsync(user);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserManagementService(usersAndRolesDbContext: userAndRolesDbContext);

                var result = await service.InviteUser("test@test.com",
                    Guid.NewGuid().ToString(),
                    new List<UserReleaseRoleAddViewModel>(),
                    new List<UserPublicationRoleAddViewModel>());

                var actionResult = result.AssertLeft();
                actionResult.AssertBadRequest(ValidationErrorMessages.UserAlreadyExists);
            }
        }

        [Fact]
        public async Task InviteUser_InvalidUserRole()
        {
                var service = SetupUserManagementService();

                var result = await service.InviteUser("test@test.com",
                    Guid.NewGuid().ToString(),
                    new List<UserReleaseRoleAddViewModel>(),
                    new List<UserPublicationRoleAddViewModel>());

                var actionResult = result.AssertLeft();
                actionResult.AssertBadRequest(ValidationErrorMessages.InvalidUserRole);
        }

        [Fact]
        public async Task CancelInvite()
        {
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await usersAndRolesDbContext.UserInvites.AddRangeAsync(
                    new UserInvite { Email = "test@test.com" },
                    new UserInvite { Email = "should.not@be.removed" });
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
                        Created = DateTime.Now,
                    },
                    new UserReleaseInvite
                    {
                        Email = "test@test.com",
                        Role = Contributor,
                        Created = DateTime.Now,
                    },
                    new UserReleaseInvite
                    {
                        Email = "should.not@be.removed",
                        Role = Lead,
                        Created = DateTime.Now,
                    });
                await contentDbContext.UserPublicationInvites.AddRangeAsync(
                    new UserPublicationInvite
                    {
                        Email = "test@test.com",
                        Role = Owner,
                        Created = DateTime.Now,
                    },
                    new UserPublicationInvite
                    {
                        Email = "test@test.com",
                        Role = PublicationRole.Approver,
                        Created = DateTime.Now,
                    },
                    new UserPublicationInvite
                    {
                        Email = "should.not@be.removed",
                        Role = Owner,
                        Created = DateTime.Now,
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
                    new UserInvite { Email = "test@test.com" });
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
                actionResult.AssertBadRequest(ValidationErrorMessages.InviteNotFound);
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
            IUserPublicationInviteRepository? userPublicationInviteRepository = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserManagementService(
                usersAndRolesDbContext,
                contentDbContext,
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
                emailTemplateService ?? Mock.Of<IEmailTemplateService>(Strict),
                userRoleService ?? Mock.Of<IUserRoleService>(Strict),
                userService ?? AlwaysTrueUserService(_createdById).Object,
                userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
                userReleaseInviteRepository ?? new UserReleaseInviteRepository(contentDbContext),
                userPublicationInviteRepository ?? new UserPublicationInviteRepository(contentDbContext)
            );
        }
    }
}
