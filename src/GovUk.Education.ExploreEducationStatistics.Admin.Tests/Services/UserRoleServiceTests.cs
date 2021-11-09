using System;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

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
        public async Task AddGlobalRole()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString()
            };

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString()
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(mock => mock.AddToRoleAsync(
                    It.Is<ApplicationUser>(
                        applicationUser => applicationUser.Id == user.Id),
                    role.Name))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.AddGlobalRole(user.Id, role.Id);

                Assert.True(result.IsRight);

                userManager.Verify(mock => mock.AddToRoleAsync(
                        It.Is<ApplicationUser>(
                            applicationUser => applicationUser.Id == user.Id),
                        role.Name),
                    Times.Once);
            }

            userManager.VerifySet(mock => mock.Logger = null, Times.Once());
            VerifyAllMocks(userManager);
        }

        [Fact]
        public async Task AddGlobalRole_NoRole()
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

                var result = await service.AddGlobalRole(user.Id, Guid.NewGuid().ToString());

                result.AssertNotFound();
            }

            userManager.VerifySet(mock => mock.Logger = null, Times.Once());
            VerifyAllMocks(userManager);
        }

        [Fact]
        public async Task AddGlobalRole_NoUser()
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

                var result = await service.AddGlobalRole(Guid.NewGuid().ToString(), role.Id);

                result.AssertNotFound();
            }

            userManager.VerifySet(mock => mock.Logger = null, Times.Once());
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

            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendPublicationRoleEmail(user.Email,
                        It.Is<Publication>(p => p.Id == publication.Id),
                        Owner))
                .Returns(Unit.Instance);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object);

                var result = await service.AddPublicationRole(userId, publication.Id, Owner);

                result.AssertRight();

                emailTemplateService.Verify(mock =>
                        mock.SendPublicationRoleEmail(user.Email,
                            It.Is<Publication>(p => p.Id == publication.Id),
                            Owner),
                    Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, userPublicationRoles[0].Id);
                Assert.Equal(userId, userPublicationRoles[0].UserId);
                Assert.Equal(publication.Id, userPublicationRoles[0].PublicationId);
                Assert.Equal(Owner, userPublicationRoles[0].Role);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationRoles[0].Created).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, userPublicationRoles[0].CreatedById);
            }

            VerifyAllMocks(emailTemplateService);
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
                Role = Owner
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
                var userPublicationRoles = await contentDbContext.UserPublicationRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Single(userPublicationRoles);

                Assert.Equal(userPublicationRole.Id, userPublicationRoles[0].Id);
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
                var userPublicationRoles = await contentDbContext.UserPublicationRoles
                    .AsQueryable()
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
                var userPublicationRoles = await contentDbContext.UserPublicationRoles
                    .AsQueryable()
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

            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            emailTemplateService.Setup(mock =>
                    mock.SendReleaseRoleEmail(user.Email,
                        It.Is<Release>(r => r.Id == release.Id),
                        Contributor))
                .Returns(Unit.Instance);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object);

                var result = await service.AddReleaseRole(userId, release.Id, Contributor);

                result.AssertRight();

                emailTemplateService.Verify(mock =>
                        mock.SendReleaseRoleEmail(user.Email,
                            It.Is<Release>(p => p.Id == release.Id),
                            Contributor),
                    Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, userReleaseRoles[0].Id);
                Assert.Equal(userId, userReleaseRoles[0].UserId);
                Assert.Equal(release.Id, userReleaseRoles[0].ReleaseId);
                Assert.Equal(Contributor, userReleaseRoles[0].Role);
                Assert.Equal(_user.Id, userReleaseRoles[0].CreatedById);
            }

            VerifyAllMocks(emailTemplateService);
        }

        [Fact]
        public async Task AddReleaseRole_UserAlreadyHasRole()
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
                Role = Contributor
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
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Single(userReleaseRoles);

                Assert.Equal(userReleaseRole.Id, userReleaseRoles[0].Id);
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
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
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
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseRoles);
            }
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

                Assert.True(result.IsRight);

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

            Assert.True(result.IsRight);

            Assert.True(result.Right.ContainsKey("Publication"));
            Assert.True(result.Right.ContainsKey("Release"));

            Assert.Single(result.Right["Publication"]);
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

                Assert.True(result.IsRight);

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
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext);

                var result = await service.GetGlobalRoles(Guid.NewGuid().ToString());
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetPublicationRoles()
        {
            var user = new User();

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

                var result = await service.GetPublicationRoles(user.Id);

                Assert.True(result.IsRight);

                var userPublicationRoles = result.Right;
                Assert.Equal(2, userPublicationRoles.Count);

                Assert.Equal(userPublicationRole1.Id, userPublicationRoles[0].Id);
                Assert.Equal("Test Publication 1", userPublicationRoles[0].Publication);
                Assert.Equal(Owner, userPublicationRoles[0].Role);

                Assert.Equal(userPublicationRole2.Id, userPublicationRoles[1].Id);
                Assert.Equal("Test Publication 2", userPublicationRoles[1].Publication);
                Assert.Equal(Owner, userPublicationRoles[1].Role);
            }
        }

        [Fact]
        public async Task GetPublicationRoles_NoUser()
        {
            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupUserRoleService(contentDbContext: contentDbContext);

                var result = await service.GetPublicationRoles(Guid.NewGuid());

                result.AssertNotFound();
            }
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
                Role = Approver
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

                Assert.True(result.IsRight);

                var userReleaseRoles = result.Right;
                Assert.Equal(2, userReleaseRoles.Count);

                Assert.Equal(userReleaseRole1.Id, userReleaseRoles[0].Id);
                Assert.Equal("Test Publication", userReleaseRoles[0].Publication);
                Assert.Equal("December 2020", userReleaseRoles[0].Release);
                Assert.Equal(Contributor, userReleaseRoles[0].Role);

                Assert.Equal(userReleaseRole2.Id, userReleaseRoles[1].Id);
                Assert.Equal("Test Publication", userReleaseRoles[1].Publication);
                Assert.Equal("June 2021", userReleaseRoles[1].Release);
                Assert.Equal(Approver, userReleaseRoles[1].Role);
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
        public async Task RemoveGlobalRole()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString()
            };

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString()
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(mock => mock.RemoveFromRoleAsync(
                    It.Is<ApplicationUser>(
                        applicationUser => applicationUser.Id == user.Id),
                    role.Name))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveGlobalRole(user.Id, role.Id);

                Assert.True(result.IsRight);

                userManager.Verify(mock => mock.RemoveFromRoleAsync(
                        It.Is<ApplicationUser>(
                            applicationUser => applicationUser.Id == user.Id),
                        role.Name),
                    Times.Once);
            }

            userManager.VerifySet(mock => mock.Logger = null, Times.Once());
            VerifyAllMocks(userManager);
        }

        [Fact]
        public async Task RemoveGlobalRole_NoUser()
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
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveGlobalRole(Guid.NewGuid().ToString(), role.Id);

                result.AssertNotFound();
            }

            userManager.VerifySet(mock => mock.Logger = null, Times.Once());
            VerifyAllMocks(userManager);
        }

        [Fact]
        public async Task RemoveGlobalRole_NoRole()
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
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object);

                var result = await service.RemoveGlobalRole(user.Id, Guid.NewGuid().ToString());

                result.AssertNotFound();
            }

            userManager.VerifySet(mock => mock.Logger = null, Times.Once());
            VerifyAllMocks(userManager);
        }

        [Fact]
        public async Task RemoveUserPublicationRole()
        {
            var userPublicationRole = new UserPublicationRole
            {
                User = new User(),
                Publication = new Publication(),
                Role = Owner
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(contentDbContext: contentDbContext);

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                Assert.True(result.IsRight);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userPublicationRoles);
            }
        }

        [Fact]
        public async Task RemoveUserPublicationRole_NoUserPublicationRole()
        {
            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupUserRoleService(contentDbContext: contentDbContext);

                var result = await service.RemoveUserPublicationRole(Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveUserReleaseRole()
        {
            var userReleaseRole = new UserReleaseRole
            {
                User = new User(),
                Release = new Release
                {
                    Publication = new Publication(),
                },
                Role = Contributor
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(contentDbContext: contentDbContext);

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseRoles);
            }
        }

        [Fact]
        public async Task RemoveUserReleaseRole_NoUserReleaseRole()
        {
            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupUserRoleService(contentDbContext: contentDbContext);

                var result = await service.RemoveUserReleaseRole(Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        private UserRoleService SetupUserRoleService(
            ContentDbContext contentDbContext = null,
            UsersAndRolesDbContext usersAndRolesDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper = null,
            IEmailTemplateService emailTemplateService = null,
            IUserPublicationRoleRepository userPublicationRoleRepository = null,
            IUserReleaseRoleRepository userReleaseRoleRepository = null,
            UserManager<ApplicationUser> userManager = null,
            IUserService userService = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            contentDbContext.Users.Add(_user);
            contentDbContext.SaveChanges();

            return new UserRoleService(
                usersAndRolesDbContext ?? InMemoryUserAndRolesDbContext(),
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
                emailTemplateService ?? new Mock<IEmailTemplateService>(MockBehavior.Strict).Object,
                userService ?? AlwaysTrueUserService(_user.Id).Object,
                userPublicationRoleRepository ?? new UserPublicationRoleRepository(contentDbContext),
                userReleaseRoleRepository ?? new UserReleaseRoleRepository(contentDbContext),
                userManager ?? MockUserManager().Object);
        }
    }
}
