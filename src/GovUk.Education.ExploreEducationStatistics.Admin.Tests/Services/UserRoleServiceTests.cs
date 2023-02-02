#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserRoleServiceTests
    {
        private readonly User _user = new()
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        [Fact]
        public async Task SetGlobalRole()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString()
            };

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role"
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager
                .Setup(mock => mock.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync(new List<string>());

            userManager.Setup(mock =>
                    mock.AddToRoleAsync(ItIsUser(user), role.Name))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.SetGlobalRole(user.Id, role.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();

                userManager.Verify(mock => mock.AddToRoleAsync(
                        It.Is<ApplicationUser>(
                            applicationUser => applicationUser.Id == user.Id),
                        role.Name),
                    Times.Once);
            }
        }

        [Fact]
        public async Task SetGlobalRole_AlreadyHasSameRole()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString()
            };

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role"
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager
                .Setup(mock => mock.GetRolesAsync(
                    ItIsUser(user)))
                .ReturnsAsync(ListOf(role.Name));

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.SetGlobalRole(user.Id, role.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();

                VerifyAllMocks(userManager);
            }
        }

        [Fact]
        public async Task SetGlobalRole_AlreadyHasAnotherRole()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString()
            };

            var newRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "New Role"
            };

            var existingRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Existing Role"
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddRangeAsync(newRole, existingRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here we are setting up the user so that they have a different Global Role currently assigned
            // than the new one being set.  They will have the existing one removed and the new one added.
            userManager
                .Setup(mock => mock.GetRolesAsync(
                    ItIsUser(user)))
                .ReturnsAsync(ListOf(existingRole.Name));

            userManager
                .Setup(mock => mock.AddToRoleAsync(
                    ItIsUser(user), newRole.Name))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(mock => mock.RemoveFromRolesAsync(
                    ItIsUser(user), ItIs.ListSequenceEqualTo(ListOf(existingRole.Name))))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.SetGlobalRole(user.Id, newRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();

                VerifyAllMocks(userManager);
            }
        }

        private static ApplicationUser ItIsUser(ApplicationUser user)
        {
            return It.Is<ApplicationUser>(
                applicationUser => applicationUser.Id == user.Id);
        }

        [Fact]
        public async Task SetGlobalRole_NoRole()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString()
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.SetGlobalRole(user.Id, Guid.NewGuid().ToString());

                result.AssertNotFound();
            }

            userManager.VerifySet(mock => mock.Logger = null, Times.Once());
            VerifyAllMocks(userManager);
        }

        [Fact]
        public async Task SetGlobalRole_NoUser()
        {
            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString()
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.SetGlobalRole(Guid.NewGuid().ToString(), role.Id);

                result.AssertNotFound();
            }

            VerifyAllMocks(userManager);
        }

        [Fact]
        public async Task AddPublicationRole()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var publication = new Publication();

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();

                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendPublicationRoleEmail(user.Email,
                        It.Is<Publication>(p => p.Id == publication.Id),
                        Owner))
                .Returns(Unit.Instance);

            var userManager = MockUserManager();

            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userManager: userManager.Object);

                var result = await service.AddPublicationRole(userId, publication.Id, Owner);

                VerifyAllMocks(emailTemplateService, userManager);

                result.AssertRight();

                emailTemplateService.Verify(mock =>
                        mock.SendPublicationRoleEmail(user.Email,
                            It.Is<Publication>(p => p.Id == publication.Id),
                            Owner),
                    Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                var assignedRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, assignedRole.Id);
                Assert.Equal(userId, assignedRole.UserId);
                Assert.Equal(publication.Id, assignedRole.PublicationId);
                Assert.Equal(Owner, assignedRole.Role);
                Assert.InRange(DateTime.UtcNow.Subtract(assignedRole.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, assignedRole.CreatedById);
            }
        }

        [Fact]
        public async Task AddPublicationRole_AssignedAnalystGlobalRole()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var publication = new Publication();

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();

                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendPublicationRoleEmail(user.Email,
                        It.Is<Publication>(p => p.Id == publication.Id),
                        Owner))
                .Returns(Unit.Instance);

            var userManager = MockUserManager();

            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync(new List<string>());

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userManager: userManager.Object);

                var result = await service.AddPublicationRole(userId, publication.Id, Owner);

                VerifyAllMocks(emailTemplateService, userManager);

                result.AssertRight();

                emailTemplateService.Verify(mock =>
                        mock.SendPublicationRoleEmail(user.Email,
                            It.Is<Publication>(p => p.Id == publication.Id),
                            Owner),
                    Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                var assignedRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, assignedRole.Id);
                Assert.Equal(userId, assignedRole.UserId);
                Assert.Equal(publication.Id, assignedRole.PublicationId);
                Assert.Equal(Owner, assignedRole.Role);
                Assert.InRange(DateTime.UtcNow.Subtract(assignedRole.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, assignedRole.CreatedById);
            }
        }

        [Fact]
        public async Task AddPublicationRole_HasHigherGlobalRoleThanAnalyst()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var publication = new Publication();

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();

                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendPublicationRoleEmail(user.Email,
                        It.Is<Publication>(p => p.Id == publication.Id),
                        Owner))
                .Returns(Unit.Instance);

            var userManager = MockUserManager();

            // Here we are testing that if the user already has a higher-powered role than Analyst, there's no need
            // to assign them the lower-powered Analyst role.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync(ListOf(RoleNames.BauUser));

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userManager: userManager.Object);

                var result = await service.AddPublicationRole(userId, publication.Id, Owner);

                VerifyAllMocks(emailTemplateService, userManager);

                result.AssertRight();

                emailTemplateService.Verify(mock =>
                        mock.SendPublicationRoleEmail(user.Email,
                            It.Is<Publication>(p => p.Id == publication.Id),
                            Owner),
                    Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                var assignedRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, assignedRole.Id);
                Assert.Equal(userId, assignedRole.UserId);
                Assert.Equal(publication.Id, assignedRole.PublicationId);
                Assert.Equal(Owner, assignedRole.Role);
                Assert.InRange(DateTime.UtcNow.Subtract(assignedRole.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, assignedRole.CreatedById);
            }
        }

        [Fact]
        public async Task AddPublicationRole_HasLowerGlobalRoleThanAnalyst()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var publication = new Publication();

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();

                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendPublicationRoleEmail(user.Email,
                        It.Is<Publication>(p => p.Id == publication.Id),
                        Owner))
                .Returns(Unit.Instance);

            var userManager = MockUserManager();

            // Here we are checking to see that the user is "upgraded" to the higher-powered Analyst role by their
            // existing PrereleaseUser role being removed and the higher-powered Analyst role being added.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync(ListOf(RoleNames.PrereleaseUser));

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(s => s.RemoveFromRolesAsync(ItIsUser(user),
                    ItIs.ListSequenceEqualTo(ListOf(RoleNames.PrereleaseUser))))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userManager: userManager.Object);

                var result = await service.AddPublicationRole(userId, publication.Id, Owner);

                VerifyAllMocks(emailTemplateService, userManager);

                result.AssertRight();

                emailTemplateService.Verify(mock =>
                        mock.SendPublicationRoleEmail(user.Email,
                            It.Is<Publication>(p => p.Id == publication.Id),
                            Owner),
                    Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                var assignedRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, assignedRole.Id);
                Assert.Equal(userId, assignedRole.UserId);
                Assert.Equal(publication.Id, assignedRole.PublicationId);
                Assert.Equal(Owner, assignedRole.Role);
                Assert.InRange(DateTime.UtcNow.Subtract(assignedRole.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, assignedRole.CreatedById);
            }
        }

        [Fact]
        public async Task AddPublicationRole_UserAlreadyHasRole()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var publication = new Publication();

            var userPublicationRole = new UserPublicationRole
            {
                UserId = userId,
                Publication = publication,
                Role = Owner,
                CreatedById = _user.Id
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();

                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext);

                var result = await service.AddPublicationRole(userId, publication.Id, Owner);

                result.AssertBadRequest(UserAlreadyHasResourceRole);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                var assignedRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, assignedRole.Id);
                Assert.Equal(userId, assignedRole.UserId);
                Assert.Equal(publication.Id, assignedRole.PublicationId);
                Assert.Equal(Owner, assignedRole.Role);
                Assert.Equal(_user.Id, assignedRole.CreatedById);
            }
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
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext);

                var result = await service.AddPublicationRole(Guid.NewGuid(), publication.Id, Owner);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                Assert.Empty(userPublicationRoles);
            }
        }

        [Fact]
        public async Task AddPublicationRole_NoPublication()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

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
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext);

                var result = await service.AddPublicationRole(userId, Guid.NewGuid(), Owner);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                Assert.Empty(userPublicationRoles);
            }
        }

        [Fact]
        public async Task AddReleaseRole()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var release = new Release
            {
                Publication = new Publication()
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();

                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendReleaseRoleEmail(user.Email,
                        It.Is<Release>(r => r.Id == release.Id),
                        Contributor))
                .Returns(Unit.Instance);

            var userManager = MockUserManager();

            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userManager: userManager.Object);

                var result = await service.AddReleaseRole(userId, release.Id, Contributor);

                VerifyAllMocks(emailTemplateService, userManager);

                result.AssertRight();

                emailTemplateService.Verify(mock =>
                        mock.SendReleaseRoleEmail(user.Email,
                            It.Is<Release>(p => p.Id == release.Id),
                            Contributor),
                    Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                var assignedRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, assignedRole.Id);
                Assert.Equal(userId, assignedRole.UserId);
                Assert.Equal(release.Id, assignedRole.ReleaseId);
                Assert.Equal(Contributor, assignedRole.Role);
                Assert.Equal(_user.Id, assignedRole.CreatedById);
            }
        }

        [Fact]
        public async Task AddReleaseRole_UserAlreadyHasReleaseRole()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var release = new Release
            {
                Publication = new Publication()
            };

            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = Contributor,
                CreatedById = _user.Id
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();

                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext);

                var result = await service.AddReleaseRole(userId, release.Id, Contributor);

                result.AssertBadRequest(UserAlreadyHasResourceRole);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                var assignedRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, assignedRole.Id);
                Assert.Equal(userId, assignedRole.UserId);
                Assert.Equal(release.Id, assignedRole.ReleaseId);
                Assert.Equal(Contributor, assignedRole.Role);
                Assert.Equal(_user.Id, assignedRole.CreatedById);
            }
        }

        [Fact]
        public async Task AddReleaseRole_NoUser()
        {
            var release = new Release
            {
                Publication = new Publication()
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext);

                var result = await service.AddReleaseRole(Guid.NewGuid(), release.Id, Contributor);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                Assert.Empty(userReleaseRoles);
            }
        }

        [Fact]
        public async Task AddReleaseRole_NoRelease()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

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
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext);

                var result = await service.AddReleaseRole(userId, Guid.NewGuid(), Contributor);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                Assert.Empty(userReleaseRoles);
            }
        }

        [Fact]
        public async Task AddReleaseRole_AssignedAnalystGlobalRole()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var release = new Release
            {
                Publication = new Publication()
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();

                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendReleaseRoleEmail(user.Email,
                        It.Is<Release>(r => r.Id == release.Id),
                        Contributor))
                .Returns(Unit.Instance);

            var userManager = MockUserManager();

            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync(new List<string>());

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userManager: userManager.Object);

                var result = await service.AddReleaseRole(userId, release.Id, Contributor);

                emailTemplateService.Verify(mock =>
                        mock.SendReleaseRoleEmail(user.Email,
                            It.Is<Release>(p => p.Id == release.Id),
                            Contributor),
                    Times.Once);
                VerifyAllMocks(emailTemplateService, userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                var assignedRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, assignedRole.Id);
                Assert.Equal(userId, assignedRole.UserId);
                Assert.Equal(release.Id, assignedRole.ReleaseId);
                Assert.Equal(Contributor, assignedRole.Role);
                Assert.Equal(_user.Id, assignedRole.CreatedById);
            }
        }

        [Fact]
        public async Task AddReleaseRole_HasLowerGlobalRoleThanAnalyst()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var release = new Release
            {
                Publication = new Publication()
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();

                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendReleaseRoleEmail(user.Email,
                        It.Is<Release>(r => r.Id == release.Id),
                        Contributor))
                .Returns(Unit.Instance);

            var userManager = MockUserManager();

            // Here we are checking to see that the user is "upgraded" to the higher-powered Analyst role by their
            // existing PrereleaseUser role being removed and the higher-powered Analyst role being added.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync(ListOf(RoleNames.PrereleaseUser));

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(s => s.RemoveFromRolesAsync(ItIsUser(user),
                        ItIs.ListSequenceEqualTo(ListOf(RoleNames.PrereleaseUser))))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userManager: userManager.Object);

                var result = await service.AddReleaseRole(userId, release.Id, Contributor);

                VerifyAllMocks(emailTemplateService, userManager);

                emailTemplateService.Verify(mock =>
                        mock.SendReleaseRoleEmail(user.Email,
                            It.Is<Release>(p => p.Id == release.Id),
                            Contributor),
                    Times.Once);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                var assignedRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, assignedRole.Id);
                Assert.Equal(userId, assignedRole.UserId);
                Assert.Equal(release.Id, assignedRole.ReleaseId);
                Assert.Equal(Contributor, assignedRole.Role);
                Assert.Equal(_user.Id, assignedRole.CreatedById);
            }
        }

        [Fact]
        public async Task AddReleaseRole_HasHigherGlobalRoleThanAnalyst()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var release = new Release
            {
                Publication = new Publication()
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();

                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendReleaseRoleEmail(user.Email,
                        It.Is<Release>(r => r.Id == release.Id),
                        Contributor))
                .Returns(Unit.Instance);

            var userManager = MockUserManager();

            // Here we are testing that if the user already has a higher-powered role than Analyst, there's no need
            // to assign them the lower-powered Analyst role.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(user)))
                .ReturnsAsync(ListOf(RoleNames.BauUser));

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userManager: userManager.Object);

                var result = await service.AddReleaseRole(userId, release.Id, Contributor);

                emailTemplateService.Verify(mock =>
                        mock.SendReleaseRoleEmail(user.Email,
                            It.Is<Release>(p => p.Id == release.Id),
                            Contributor),
                    Times.Once);
                VerifyAllMocks(emailTemplateService, userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                var assignedRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, assignedRole.Id);
                Assert.Equal(userId, assignedRole.UserId);
                Assert.Equal(release.Id, assignedRole.ReleaseId);
                Assert.Equal(Contributor, assignedRole.Role);
                Assert.Equal(_user.Id, assignedRole.CreatedById);
            }
        }

        [Fact]
        public async Task UpgradeToGlobalRoleIfRequired_DoUpgrade()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var existingRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = RoleNames.PrereleaseUser,
            };

            var newRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = RoleNames.Analyst,
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddRangeAsync(user);
                await userAndRolesDbContext.Roles.AddRangeAsync(existingRole, newRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here we are setting up the user so that they have a different Global Role currently assigned
            // than the new one being set. They will have the existing one removed and the new one added.
            userManager
                .Setup(mock => mock.GetRolesAsync(
                    ItIsUser(user)))
                .ReturnsAsync(ListOf(existingRole.Name));

            userManager
                .Setup(mock => mock.AddToRoleAsync(
                    ItIsUser(user), newRole.Name))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(mock => mock.RemoveFromRolesAsync(
                    ItIsUser(user), ItIs.ListSequenceEqualTo(ListOf(existingRole.Name))))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.UpgradeToGlobalRoleIfRequired(RoleNames.Analyst, userId);
                result.AssertRight();

                VerifyAllMocks(userManager);
            }
        }

        [Fact]
        public async Task UpgradeToGlobalRoleIfRequired_DoNotUpgrade()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId.ToString()
            };

            var existingRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = RoleNames.Analyst,
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddRangeAsync(user);
                await userAndRolesDbContext.Roles.AddRangeAsync(existingRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // user already has the correct role
            userManager
                .Setup(mock => mock.GetRolesAsync(
                    ItIsUser(user)))
                .ReturnsAsync(ListOf(existingRole.Name));

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.UpgradeToGlobalRoleIfRequired(RoleNames.Analyst, userId);
                result.AssertRight();

                VerifyAllMocks(userManager);
            }
        }

        [Fact]
        public async Task UpgradeToGlobalRoleIfRequired_NoUser()
        {
            var service = SetupUserRoleService();

            var result = await service.UpgradeToGlobalRoleIfRequired(RoleNames.Analyst, Guid.NewGuid());
            result.AssertNotFound();
        }

        [Fact]
        public async Task GetAllGlobalRoles()
        {
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

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddRangeAsync(role1, role2);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext);

                var result = await service.GetAllGlobalRoles();

                result.AssertRight();

                var globalRoles = result.Right.ToList();
                Assert.Equal(2, globalRoles.Count);

                Assert.Equal(role1.Id, globalRoles[0].Id);
                Assert.Equal(role1.Name, globalRoles[0].Name);
                Assert.Equal(role1.NormalizedName, globalRoles[0].NormalizedName);

                Assert.Equal(role2.Id, globalRoles[1].Id);
                Assert.Equal(role2.Name, globalRoles[1].Name);
                Assert.Equal(role2.NormalizedName, globalRoles[1].NormalizedName);
            }
        }

        [Fact]
        public async Task GetAllResourceRoles()
        {
            var service = SetupUserRoleService();

            var result = await service.GetAllResourceRoles();

            result.AssertRight();

            Assert.True(result.Right.ContainsKey("Publication"));
            Assert.True(result.Right.ContainsKey("Release"));

            Assert.Equal(2, result.Right["Publication"].Count);
            Assert.Equal(5, result.Right["Release"].Count);
        }

        [Fact]
        public async Task GetGlobalRoles()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString()
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

            // Role not assigned
            var role3 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 3",
                NormalizedName = "ROLE 3"
            };

            var userRole1 = new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = role1.Id
            };

            var userRole2 = new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = role2.Id
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddRangeAsync(role1, role2, role3);
                await userAndRolesDbContext.AddRangeAsync(userRole1, userRole2);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext);

                var result = await service.GetGlobalRoles(user.Id);

                result.AssertRight();

                var globalRoles = result.Right;
                Assert.Equal(2, globalRoles.Count);

                Assert.Equal(role1.Id, globalRoles[0].Id);
                Assert.Equal(role1.Name, globalRoles[0].Name);
                Assert.Equal(role1.NormalizedName, globalRoles[0].NormalizedName);

                Assert.Equal(role2.Id, globalRoles[1].Id);
                Assert.Equal(role2.Name, globalRoles[1].Name);
                Assert.Equal(role2.NormalizedName, globalRoles[1].NormalizedName);
            }
        }

        [Fact]
        public async Task GetGlobalRoles_NoUser()
        {
            await using var userAndRolesDbContext = InMemoryUserAndRolesDbContext();
            var service = SetupUserRoleService(
                usersAndRolesDbContext: userAndRolesDbContext);

            var result = await service.GetGlobalRoles(Guid.NewGuid().ToString());
            result.AssertNotFound();
        }

        [Fact]
        public async Task GetPublicationRolesForUser()
        {
            var user = new User
            {
                FirstName = "User",
                LastName = "1",
                Email = "user1@example.com"
            };

            var userPublicationRole1 = new UserPublicationRole
            {
                User = user,
                Publication = new Publication
                {
                    Title = "Test Publication 1"
                },
                Role = Owner
            };

            var userPublicationRole2 = new UserPublicationRole
            {
                User = user,
                Publication = new Publication
                {
                    Title = "Test Publication 2"
                },
                Role = Owner
            };

            // Role assignment for a different user
            var userPublicationRole3 = new UserPublicationRole
            {
                User = new User(),
                Publication = new Publication
                {
                    Title = "Test Publication 3"
                },
                Role = Owner
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    userPublicationRole1,
                    userPublicationRole2,
                    userPublicationRole3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(contentDbContext: contentDbContext);

                var result = await service.GetPublicationRolesForUser(user.Id);

                result.AssertRight();

                var userPublicationRoles = result.Right;
                Assert.Equal(2, userPublicationRoles.Count);

                Assert.Equal(userPublicationRole1.Id, userPublicationRoles[0].Id);
                Assert.Equal("Test Publication 1", userPublicationRoles[0].Publication);
                Assert.Equal("User 1", userPublicationRoles[0].UserName);
                Assert.Equal(Owner, userPublicationRoles[0].Role);
                Assert.Equal("user1@example.com", userPublicationRoles[0].Email);

                Assert.Equal(userPublicationRole2.Id, userPublicationRoles[1].Id);
                Assert.Equal("Test Publication 2", userPublicationRoles[1].Publication);
                Assert.Equal("User 1", userPublicationRoles[1].UserName);
                Assert.Equal(Owner, userPublicationRoles[1].Role);
                Assert.Equal("user1@example.com", userPublicationRoles[1].Email);
            }
        }

        [Fact]
        public async Task GetPublicationRolesForUser_NoUser()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupUserRoleService(contentDbContext: contentDbContext);

            var result = await service.GetPublicationRolesForUser(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetPublicationRolesForPublication()
        {
            var user1 = new User
            {
                FirstName = "User",
                LastName = "1",
                Email = "user1@example.com"
            };
            
            var user2 = new User
            {
                FirstName = "User",
                LastName = "2",
                Email = "user2@example.com"
            };
            
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Title = "Test Publication 1"
            };

            var userPublicationRole1 = new UserPublicationRole
            {
                User = user2,
                Publication = publication,
                Role = Owner
            };

            var userPublicationRole2 = new UserPublicationRole
            {
                User = user1,
                Publication = publication,
                Role = Owner
            };

            // Role assignment for a different publication
            var userPublicationRole3 = new UserPublicationRole
            {
                User = user1,
                Publication = new Publication(),
                Role = Owner
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddRangeAsync(user1, user2);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    userPublicationRole1,
                    userPublicationRole2,
                    userPublicationRole3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(contentDbContext: contentDbContext);

                var result = await service.GetPublicationRolesForPublication(publication.Id);

                result.AssertRight();

                var userPublicationRoles = result.Right;
                Assert.Equal(2, userPublicationRoles.Count);

                Assert.Equal(userPublicationRole2.Id, userPublicationRoles[0].Id);
                Assert.Equal("Test Publication 1", userPublicationRoles[0].Publication);
                Assert.Equal("User 1", userPublicationRoles[0].UserName);
                Assert.Equal(Owner, userPublicationRoles[0].Role);
                Assert.Equal("user1@example.com", userPublicationRoles[0].Email);

                Assert.Equal(userPublicationRole1.Id, userPublicationRoles[1].Id);
                Assert.Equal("Test Publication 1", userPublicationRoles[1].Publication);
                Assert.Equal("User 2", userPublicationRoles[1].UserName);
                Assert.Equal(Owner, userPublicationRoles[1].Role);
                Assert.Equal("user2@example.com", userPublicationRoles[1].Email);
            }
        }

        [Fact]
        public async Task GetPublicationRolesForPublication_NoPublication()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupUserRoleService(contentDbContext: contentDbContext);

            var result = await service.GetPublicationRolesForPublication(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetReleaseRoles()
        {
            var user = new User();
            var publication = new Publication
            {
                Title = "Test Publication"
            };

            var userReleaseRole1 = new UserReleaseRole
            {
                User = user,
                Release = new Release
                {
                    Publication = publication,
                    ReleaseName = "2020",
                    TimePeriodCoverage = December
                },
                Role = Contributor
            };

            var userReleaseRole2 = new UserReleaseRole
            {
                User = user,
                Release = new Release
                {
                    Publication = publication,
                    ReleaseName = "2021",
                    TimePeriodCoverage = June
                },
                Role = ReleaseRole.Approver
            };

            // Role assignment for a different user
            var userReleaseRole3 = new UserReleaseRole
            {
                User = new User(),
                Release = new Release
                {
                    Publication = publication,
                    ReleaseName = "2022",
                    TimePeriodCoverage = July
                },
                Role = Contributor
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(
                    userReleaseRole1,
                    userReleaseRole2,
                    userReleaseRole3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(contentDbContext: contentDbContext);

                var result = await service.GetReleaseRoles(user.Id);

                result.AssertRight();

                var userReleaseRoles = result.Right;
                Assert.Equal(2, userReleaseRoles.Count);

                Assert.Equal(userReleaseRole1.Id, userReleaseRoles[0].Id);
                Assert.Equal("Test Publication", userReleaseRoles[0].Publication);
                Assert.Equal("December 2020", userReleaseRoles[0].Release);
                Assert.Equal(Contributor, userReleaseRoles[0].Role);

                Assert.Equal(userReleaseRole2.Id, userReleaseRoles[1].Id);
                Assert.Equal("Test Publication", userReleaseRoles[1].Publication);
                Assert.Equal("June 2021", userReleaseRoles[1].Release);
                Assert.Equal(ReleaseRole.Approver, userReleaseRoles[1].Role);
            }
        }

        [Fact]
        public async Task GetReleaseRoles_NoUser()
        {
            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupUserRoleService(contentDbContext: contentDbContext);

                var result = await service.GetReleaseRoles(Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveUserPublicationRole()
        {
            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = Owner
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            userManager
                .Setup(s => s.RemoveFromRolesAsync(
                    ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst))))
                .ReturnsAsync(new IdentityResult());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                Assert.Empty(userPublicationRoles);
            }
        }

        [Fact]
        public async Task RemoveUserPublicationRole_NoUserPublicationRole()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupUserRoleService(contentDbContext: contentDbContext);

            var result = await service.RemoveUserPublicationRole(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task RemoveUserPublicationRole_HasHigherGlobalRoleThanAnalyst()
        {
            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = Owner
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the BAU role, which is higher-powered than Analyst, so we will test that they remain
            // a BAU user and do not have any roles removed.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.BauUser));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                Assert.Empty(userPublicationRoles);
            }
        }

        [Fact]
        public async Task RemoveUserPublicationRole_AnalystRoleStillRequiredForOtherPublications()
        {
            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = Owner
            };

            var anotherUserPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = Owner
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    userPublicationRole, anotherUserPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they retain the Analyst role  because
            // they still have need of it in other Publication roles.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                var remainingPublicationRole = Assert.Single(userPublicationRoles);
                Assert.Equal(anotherUserPublicationRole.PublicationId, remainingPublicationRole.PublicationId);
            }
        }

        [Fact]
        public async Task RemoveUserPublicationRole_AnalystRoleStillRequiredForOtherReleases()
        {
            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = Owner
            };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                Role = ReleaseRole.Approver
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they retain the Analyst role because
            // they still have need of it in other Release roles.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                Assert.Empty(userPublicationRoles);
            }
        }

        [Fact]
        public async Task RemoveUserPublicationRole_DowngradeToPrereleaseRole()
        {
            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = Owner
            };

            var prereleaseRole = new UserReleaseRole
            {
                User = user,
                Role = PrereleaseViewer
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.UserReleaseRoles.AddAsync(prereleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they lose the Analyst role but gain the
            // PrereleaseUser role, as they have need of this role elsewhere.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            userManager
                .Setup(s => s.RemoveFromRolesAsync(
                    ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst))))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(identityUser), RoleNames.PrereleaseUser))
                .ReturnsAsync(new IdentityResult());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                Assert.Empty(userPublicationRoles);
            }
        }

        [Fact]
        public async Task RemoveUserReleaseRole()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                Release = release,
                Role = ReleaseRole.Approver
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            userManager
                .Setup(s => s.RemoveFromRolesAsync(
                    ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst))))
                .ReturnsAsync(new IdentityResult());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                Assert.Empty(userReleaseRoles);
            }
        }

        [Fact]
        public async Task RemoveUserReleaseRole_RemoveAssociatedUserReleaseInvite()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                Release = release,
                Role = ReleaseRole.Approver
            };

            var userReleaseInvite = new UserReleaseInvite
            {
                Release = userReleaseRole.Release,
                Email = userReleaseRole.User.Email,
                Role = userReleaseRole.Role
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.UserReleaseInvites.AddAsync(userReleaseInvite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            userManager
                .Setup(s => s.RemoveFromRolesAsync(
                    ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst))))
                .ReturnsAsync(new IdentityResult());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userReleaseInviteRepository: new UserReleaseInviteRepository(contentDbContext),
                    userManager: userManager.Object);

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                Assert.Empty(userReleaseRoles);

                var userReleaseInvites = await contentDbContext
                    .UserReleaseInvites
                    .ToListAsync();

                Assert.Empty(userReleaseInvites);
            }
        }

        [Fact]
        public async Task RemoveUserReleaseRole_NoUserReleaseRole()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupUserRoleService(contentDbContext: contentDbContext);

            var result = await service.RemoveUserReleaseRole(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task RemoveUserReleaseRole_HasHigherGlobalRoleThanAnalyst()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                Release = release,
                Role = ReleaseRole.Approver
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the BAU role, which is higher-powered than Analyst, so we will test that they remain
            // a BAU user and do not have any roles removed.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.BauUser));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                Assert.Empty(userReleaseRoles);
            }
        }

        [Fact]
        public async Task RemoveUserReleaseRole_AnalystRoleStillRequiredForOtherReleases()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                Release = release,
                Role = ReleaseRole.Approver
            };

            var anotherUserReleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseId = Guid.NewGuid(),
                Role = ReleaseRole.Approver
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddRangeAsync(
                    userReleaseRole, anotherUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they retain the Analyst role  because
            // they still have need of it in other Release roles.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                var remainingReleaseRole = Assert.Single(userReleaseRoles);
                Assert.Equal(anotherUserReleaseRole.ReleaseId, remainingReleaseRole.ReleaseId);
            }
        }

        [Fact]
        public async Task RemoveUserReleaseRole_AnalystRoleStillRequiredForOtherPublications()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var user = new User
            {
                Id = Guid.NewGuid()
            };
            
            var analystGlobalRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = Role.Analyst.ToString()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };
            
            var identityUserRole = new IdentityUserRole<string>
            {
                UserId = identityUser.Id,
                RoleId = analystGlobalRole.Id
            };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                Release = release,
                Role = ReleaseRole.Approver
            };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                Role = Owner
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.Roles.AddAsync(analystGlobalRole);
                await userAndRolesDbContext.UserRoles.AddAsync(identityUserRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they retain the Analyst role because
            // they still have need of it in other Publication roles.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                Assert.Empty(userReleaseRoles);
                
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .ToListAsync();

                Assert.Single(userPublicationRoles);
            }
            
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var userGlobalRoles = await userAndRolesDbContext
                    .UserRoles
                    .ToListAsync();

                var globalRole = Assert.Single(userGlobalRoles);
                
                Assert.Equal(analystGlobalRole.Id, globalRole.RoleId);
            }
        }

        [Fact]
        public async Task RemoveUserReleaseRole_DowngradeToPrereleaseRole()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var anotherRelease = new Release
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                Release = release,
                Role = ReleaseRole.Approver
            };

            var prereleaseRole = new UserReleaseRole
            {
                User = user,
                Release = anotherRelease,
                Role = PrereleaseViewer
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRole, prereleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they lose the Analyst role but gain the
            // PrereleaseUser role, as they have need of this role elsewhere.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            userManager
                .Setup(s => s.RemoveFromRolesAsync(
                    ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst))))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(identityUser), RoleNames.PrereleaseUser))
                .ReturnsAsync(new IdentityResult());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                var releaseRole = Assert.Single(userReleaseRoles);
                Assert.Equal(prereleaseRole.ReleaseId, releaseRole.ReleaseId);
            }
        }

        [Fact]
        public async Task RemoveUserReleaseRoleAsPrerelease_AnalystRoleRetained()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var approverRole = new UserReleaseRole
            {
                User = user,
                Release = release,
                Role = ReleaseRole.Approver
            };

            var prereleaseRole = new UserReleaseRole
            {
                User = user,
                Release = release,
                Role = PrereleaseViewer
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddRangeAsync(approverRole, prereleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently but now we are removing a Prerelease Release Role.
            // We will test that they retain the Analyst role.
            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveUserReleaseRole(prereleaseRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                var releaseRole = Assert.Single(userPublicationRoles);
                Assert.Equal(approverRole.ReleaseId, releaseRole.ReleaseId);
            }
        }

        [Fact]
        public async Task RemoveAllUserResourceRoles()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var user = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUser = new ApplicationUser
            {
                Id = user.Id.ToString()
            };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                Release = release,
                Role = ReleaseRole.Approver
            };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                Publication = release.Publication,
                Role = PublicationRole.Approver
            };

            var userTwo = new User
            {
                Id = Guid.NewGuid()
            };

            var identityUserTwo = new ApplicationUser
            {
                Id = userTwo.Id.ToString()
            };

            var userTwoReleaseRole = new UserReleaseRole
            {
                User = userTwo,
                Release = release,
                Role = ReleaseRole.Approver
            };

            var userTwoPublicationRole = new UserPublicationRole
            {
                User = userTwo,
                Publication = release.Publication,
                Role = PublicationRole.Approver
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRole, userTwoReleaseRole);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(userPublicationRole, userTwoPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddRangeAsync(identityUser, identityUserTwo);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager
                .Setup(s => s.GetRolesAsync(ItIsUser(identityUser)))
                .ReturnsAsync(ListOf(RoleNames.Analyst));

            userManager
                .Setup(s => s.RemoveFromRolesAsync(
                    ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst))))
                .ReturnsAsync(new IdentityResult());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveAllUserResourceRoles(user.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .ToListAsync();

                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.ToListAsync();

                // At this point none of userTwo's roles should be affected.
                var remainingUserReleaseRole = Assert.Single(userReleaseRoles);
                var remainingUserPublicationRole = Assert.Single(userPublicationRoles);

                Assert.Equal(userTwo.Id, remainingUserReleaseRole.UserId);
                Assert.Equal(userTwo.Id, remainingUserPublicationRole.UserId);
            }
        }

        private UserRoleService SetupUserRoleService(
            ContentDbContext? contentDbContext = null,
            UsersAndRolesDbContext? usersAndRolesDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
            IEmailTemplateService? emailTemplateService = null,
            IUserPublicationRoleRepository? userPublicationRoleRepository = null,
            IUserReleaseRoleRepository? userReleaseRoleRepository = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null,
            UserManager<ApplicationUser>? userManager = null,
            IUserService? userService = null)
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
                emailTemplateService ?? new Mock<IEmailTemplateService>(Strict).Object,
                userService ?? AlwaysTrueUserService(_user.Id).Object,
                userPublicationRoleRepository ?? new UserPublicationRoleRepository(contentDbContext),
                userReleaseRoleRepository ?? new UserReleaseRoleRepository(contentDbContext),
                userReleaseInviteRepository ?? new UserReleaseInviteRepository(contentDbContext),
                userManager ?? MockUserManager().Object);
        }
    }
}
