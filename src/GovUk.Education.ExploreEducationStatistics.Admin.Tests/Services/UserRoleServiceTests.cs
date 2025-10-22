#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserRoleServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private readonly User _user = new DataFixture().DefaultUser().WithId(Guid.NewGuid());

    public class SetGlobalRoleTests : UserRoleServiceTests
    {
        [Fact]
        public async Task SetGlobalRole()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

            var role = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Role" };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(mock => mock.GetRolesAsync(ItIsUser(user))).ReturnsAsync(new List<string>());

            userManager
                .Setup(mock => mock.AddToRoleAsync(ItIsUser(user), role.Name))
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                var result = await service.SetGlobalRole(user.Id, role.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();

                userManager.Verify(
                    mock =>
                        mock.AddToRoleAsync(
                            It.Is<ApplicationUser>(applicationUser => applicationUser.Id == user.Id),
                            role.Name
                        ),
                    Times.Once
                );
            }
        }

        [Fact]
        public async Task SetGlobalRole_AlreadyHasSameRole()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

            var role = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Role" };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(mock => mock.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(role.Name));

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                var result = await service.SetGlobalRole(user.Id, role.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();

                VerifyAllMocks(userManager);
            }
        }

        [Fact]
        public async Task SetGlobalRole_AlreadyHasAnotherRole()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

            var newRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "New Role" };

            var existingRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Existing Role" };

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
            userManager.Setup(mock => mock.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(existingRole.Name));

            userManager
                .Setup(mock => mock.AddToRoleAsync(ItIsUser(user), newRole.Name))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(mock =>
                    mock.RemoveFromRolesAsync(ItIsUser(user), ItIs.ListSequenceEqualTo(ListOf(existingRole.Name)))
                )
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                var result = await service.SetGlobalRole(user.Id, newRole.Id);

                VerifyAllMocks(userManager);

                result.AssertRight();

                VerifyAllMocks(userManager);
            }
        }

        [Fact]
        public async Task SetGlobalRole_NoRole()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

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
                    userManager: userManager.Object
                );

                var result = await service.SetGlobalRole(user.Id, Guid.NewGuid().ToString());

                result.AssertNotFound();
            }

            userManager.VerifySet(mock => mock.Logger = null, Times.Once());
            VerifyAllMocks(userManager);
        }

        [Fact]
        public async Task SetGlobalRole_NoUser()
        {
            var role = new IdentityRole { Id = Guid.NewGuid().ToString() };

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
                    userManager: userManager.Object
                );

                var result = await service.SetGlobalRole(Guid.NewGuid().ToString(), role.Id);

                result.AssertNotFound();
            }

            VerifyAllMocks(userManager);
        }
    }

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
                await userAndRolesDbContext.Users.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPublicationRole(
                        _user.Id,
                        It.Is<Publication>(p => p.Id == publication.Id),
                        PublicationRole.Owner,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.Analyst));

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.UserHasRoleOnPublication(_user.Id, publication.Id, PublicationRole.Owner))
                .ReturnsAsync(false);
            userPublicationRoleRepository
                .Setup(s => s.Create(_user.Id, publication.Id, PublicationRole.Owner, _user.Id))
                .ReturnsAsync(new UserPublicationRole());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Owner);

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userManager, userPublicationRoleRepository);
        }

        [Fact]
        public async Task AddPublicationRole_HasNoRoles_UpgradesRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = Mock.Of<IUserPublicationRoleRepository>();

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPublicationRole(
                        _user.Id,
                        It.Is<Publication>(p => p.Id == publication.Id),
                        PublicationRole.Owner,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(new List<string>());

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Owner);

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userManager);
        }

        [Fact]
        public async Task AddPublicationRole_HasHigherGlobalRoleThanAnalyst_DoesNotUpgradeRole()
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

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPublicationRole(
                        _user.Id,
                        It.Is<Publication>(p => p.Id == publication.Id),
                        PublicationRole.Owner,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = Mock.Of<IUserPublicationRoleRepository>();

            var userManager = MockUserManager();

            // Here we are testing that if the user already has a higher-powered role than Analyst, there's no need
            // to assign them the lower-powered Analyst role.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.BauUser));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository,
                    userManager: userManager.Object
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Owner);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userResourceRoleNotificationService);

            userManager.Verify(
                mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
                Times.Never
            );

            userManager.Verify(
                mock => mock.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<List<string>>()),
                Times.Never
            );
        }

        [Fact]
        public async Task AddPublicationRole_HasLowerGlobalRoleThanAnalyst_UpgradesRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = Mock.Of<IUserPublicationRoleRepository>();

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPublicationRole(
                        _user.Id,
                        It.Is<Publication>(p => p.Id == publication.Id),
                        PublicationRole.Owner,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var userManager = MockUserManager();

            // Here we are checking to see that the user is "upgraded" to the higher-powered Analyst role by their
            // existing PrereleaseUser role being removed and the higher-powered Analyst role being added.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.PrereleaseUser));

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(user), ItIs.ListSequenceEqualTo(ListOf(RoleNames.PrereleaseUser)))
                )
                .ReturnsAsync(new IdentityResult());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Owner);

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userManager);
        }

        [Fact]
        public async Task AddPublicationRole_UserAlreadyHasRole()
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
                .Setup(mock => mock.UserHasRoleOnPublication(_user.Id, publication.Id, PublicationRole.Owner))
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.AddPublicationRole(_user.Id, publication.Id, PublicationRole.Owner);

                result.AssertBadRequest(UserAlreadyHasResourceRole);
            }

            VerifyAllMocks(userPublicationRoleRepository);

            userPublicationRoleRepository.Verify(
                s => s.Create(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PublicationRole>(), It.IsAny<Guid>()),
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
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext
                );

                var result = await service.AddPublicationRole(Guid.NewGuid(), publication.Id, PublicationRole.Owner);

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
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext
                );

                var result = await service.AddPublicationRole(userId, Guid.NewGuid(), PublicationRole.Owner);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                Assert.Empty(userPublicationRoles);
            }
        }
    }

    public class AddReleaseRoleTests : UserRoleServiceTests
    {
        [Fact]
        public async Task AddReleaseRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

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

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewReleaseRole(
                        _user.Id,
                        It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id),
                        ReleaseRole.Contributor,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync([RoleNames.Analyst]);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(mock => mock.HasUserReleaseRole(_user.Id, releaseVersion.Id, ReleaseRole.Contributor))
                .ReturnsAsync(false);
            userReleaseRoleRepository
                .Setup(s => s.Create(_user.Id, releaseVersion.Id, ReleaseRole.Contributor, _user.Id))
                .ReturnsAsync(new UserReleaseRole());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.AddReleaseRole(
                    userId: _user.Id,
                    releaseId: release.Id,
                    ReleaseRole.Contributor
                );

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userManager, userReleaseRoleRepository);
        }

        [Fact]
        public async Task AddReleaseRole_UserAlreadyHasReleaseRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            var userReleaseRole = new UserReleaseRole
            {
                UserId = _user.Id,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.Contributor,
                CreatedById = _user.Id,
            };

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
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository
                .Setup(mock => mock.HasUserReleaseRole(_user.Id, release.Versions[0].Id, ReleaseRole.Contributor))
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.AddReleaseRole(
                    userId: _user.Id,
                    releaseId: release.Id,
                    ReleaseRole.Contributor
                );

                result.AssertBadRequest(UserAlreadyHasResourceRole);
            }

            VerifyAllMocks(userReleaseRoleRepository);
        }

        [Fact]
        public async Task AddReleaseRole_NoUser()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository
                .Setup(mock => mock.HasUserReleaseRole(_user.Id, release.Versions[0].Id, ReleaseRole.Contributor))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.AddReleaseRole(
                    userId: _user.Id,
                    releaseId: release.Id,
                    ReleaseRole.Contributor
                );

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                Assert.Empty(userReleaseRoles);
            }
        }

        [Fact]
        public async Task AddReleaseRole_NoRelease()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser { Id = userId.ToString() };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext
                );

                var result = await service.AddReleaseRole(
                    userId: userId,
                    releaseId: Guid.NewGuid(),
                    ReleaseRole.Contributor
                );

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                Assert.Empty(userReleaseRoles);
            }
        }

        [Fact]
        public async Task AddReleaseRole_HasNoRoles_UpgradesRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

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

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewReleaseRole(
                        _user.Id,
                        It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id),
                        ReleaseRole.Contributor,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync([]);

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            var userReleaseRoleRepository = Mock.Of<IUserReleaseRoleRepository>();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository
                );

                var result = await service.AddReleaseRole(
                    userId: _user.Id,
                    releaseId: release.Id,
                    ReleaseRole.Contributor
                );

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userManager);
        }

        [Fact]
        public async Task AddReleaseRole_HasLowerGlobalRoleThanAnalyst_UpgradesRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

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

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewReleaseRole(
                        _user.Id,
                        It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id),
                        ReleaseRole.Contributor,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var userManager = MockUserManager();

            // Here we are checking to see that the user is "upgraded" to the higher-powered Analyst role by their
            // existing PrereleaseUser role being removed and the higher-powered Analyst role being added.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.PrereleaseUser));

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(user), RoleNames.Analyst))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(user), ItIs.ListSequenceEqualTo(ListOf(RoleNames.PrereleaseUser)))
                )
                .ReturnsAsync(new IdentityResult());

            var userReleaseRoleRepository = Mock.Of<IUserReleaseRoleRepository>();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository
                );

                var result = await service.AddReleaseRole(
                    userId: _user.Id,
                    releaseId: release.Id,
                    ReleaseRole.Contributor
                );

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userManager);
        }

        [Fact]
        public async Task AddReleaseRole_HasHigherGlobalRoleThanAnalyst_DoesNotUpgradeRole()
        {
            var user = new ApplicationUser { Id = _user.Id.ToString(), Email = _user.Email };

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

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

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewReleaseRole(
                        _user.Id,
                        It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id),
                        ReleaseRole.Contributor,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var userManager = MockUserManager();

            // Here we are testing that if the user already has a higher-powered role than Analyst, there's no need
            // to assign them the lower-powered Analyst role.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(RoleNames.BauUser));

            var userReleaseRoleRepository = Mock.Of<IUserReleaseRoleRepository>();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userManager: userManager.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository
                );

                var result = await service.AddReleaseRole(
                    userId: _user.Id,
                    releaseId: release.Id,
                    ReleaseRole.Contributor
                );

                result.AssertRight();
            }

            VerifyAllMocks(userResourceRoleNotificationService, userManager);
        }
    }

    public class UpgradeToGlobalRoleIfRequiredTests : UserRoleServiceTests
    {
        [Fact]
        public async Task UpgradeToGlobalRoleIfRequired_DoUpgrade()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId.ToString() };

            var existingRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = RoleNames.PrereleaseUser };

            var newRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = RoleNames.Analyst };

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
            userManager.Setup(mock => mock.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(existingRole.Name));

            userManager
                .Setup(mock => mock.AddToRoleAsync(ItIsUser(user), newRole.Name))
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(mock =>
                    mock.RemoveFromRolesAsync(ItIsUser(user), ItIs.ListSequenceEqualTo(ListOf(existingRole.Name)))
                )
                .ReturnsAsync(new IdentityResult());

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

                var result = await service.UpgradeToGlobalRoleIfRequired(RoleNames.Analyst, userId);
                result.AssertRight();

                VerifyAllMocks(userManager);
            }
        }

        [Fact]
        public async Task UpgradeToGlobalRoleIfRequired_DoNotUpgrade()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId.ToString() };

            var existingRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = RoleNames.Analyst };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddRangeAsync(user);
                await userAndRolesDbContext.Roles.AddRangeAsync(existingRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // user already has the correct role
            userManager.Setup(mock => mock.GetRolesAsync(ItIsUser(user))).ReturnsAsync(ListOf(existingRole.Name));

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object
                );

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
    }

    public class GetAllGlobalRolesTests : UserRoleServiceTests
    {
        [Fact]
        public async Task GetAllGlobalRoles()
        {
            var role1 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1",
            };

            var role2 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 2",
                NormalizedName = "ROLE 2",
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddRangeAsync(role1, role2);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext);

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
    }

    public class GetAllResourceRolesTests : UserRoleServiceTests
    {
        [Fact]
        public async Task GetAllResourceRoles()
        {
            var service = SetupUserRoleService();

            var result = await service.GetAllResourceRoles();

            var resourceRoles = result.AssertRight();

            Assert.True(resourceRoles.ContainsKey("Publication"));
            Assert.True(resourceRoles.ContainsKey("Release"));

            Assert.Equal(2, resourceRoles["Publication"].Count);
            Assert.Equal(3, resourceRoles["Release"].Count);

            Assert.Contains(nameof(PublicationRole.Owner), resourceRoles["Publication"]);
            Assert.Contains(nameof(PublicationRole.Allower), resourceRoles["Publication"]);

            Assert.Contains(nameof(ReleaseRole.Contributor), resourceRoles["Release"]);
            Assert.Contains(nameof(ReleaseRole.Approver), resourceRoles["Release"]);
            Assert.Contains(nameof(ReleaseRole.PrereleaseViewer), resourceRoles["Release"]);
        }
    }

    public class GetGlobalRolesTests : UserRoleServiceTests
    {
        [Fact]
        public async Task GetGlobalRoles()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString() };

            var role1 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1",
            };

            var role2 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 2",
                NormalizedName = "ROLE 2",
            };

            // Role not assigned
            var role3 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 3",
                NormalizedName = "ROLE 3",
            };

            var userRole1 = new IdentityUserRole<string> { UserId = user.Id, RoleId = role1.Id };

            var userRole2 = new IdentityUserRole<string> { UserId = user.Id, RoleId = role2.Id };

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
                var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext);

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
            var service = SetupUserRoleService(usersAndRolesDbContext: userAndRolesDbContext);

            var result = await service.GetGlobalRoles(Guid.NewGuid().ToString());
            result.AssertNotFound();
        }
    }

    public class GetPublicationRolesForUserTests : UserRoleServiceTests
    {
        [Fact]
        public async Task GetPublicationRolesForUser()
        {
            User user = _dataFixture.DefaultUser().WithFirstName("User").WithLastName("1").WithEmail("user1@example.com");

            var userPublicationRole1 = new UserPublicationRole
            {
                User = user,
                Publication = new Publication { Title = "Test Publication 1" },
                Role = PublicationRole.Owner,
            };

            var userPublicationRole2 = new UserPublicationRole
            {
                User = user,
                Publication = new Publication { Title = "Test Publication 2" },
                Role = PublicationRole.Owner,
            };

            // Role assignment for a different user
            var userPublicationRole3 = new UserPublicationRole
            {
                User = _dataFixture.DefaultUser(),
                Publication = new Publication { Title = "Test Publication 3" },
                Role = PublicationRole.Owner,
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    userPublicationRole1,
                    userPublicationRole2,
                    userPublicationRole3
                );
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
                Assert.Equal(PublicationRole.Owner, userPublicationRoles[0].Role);
                Assert.Equal("user1@example.com", userPublicationRoles[0].Email);

                Assert.Equal(userPublicationRole2.Id, userPublicationRoles[1].Id);
                Assert.Equal("Test Publication 2", userPublicationRoles[1].Publication);
                Assert.Equal("User 1", userPublicationRoles[1].UserName);
                Assert.Equal(PublicationRole.Owner, userPublicationRoles[1].Role);
                Assert.Equal("user1@example.com", userPublicationRoles[1].Email);
            }
        }

        // This test will be changed when we start introducing the use of the NEW publication roles in the
        // UI, in STEP 9 (EES-6196) of the Permissions Rework. For now, we want to
        // filter out any usage of the NEW roles.
        [Fact]
        public async Task GetPublicationRolesForUser_InvalidRolesNotReturned()
        {
            User user = _dataFixture
                .DefaultUser()
                .WithFirstName("User")
                .WithLastName("1")
                .WithEmail("user1@example.com");

            UserPublicationRole userPublicationRole1 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Approver);

            UserPublicationRole userPublicationRole2 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            // Role assignment for a different user
            UserPublicationRole userPublicationRole3 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser().Generate())
                .WithPublication(_dataFixture.DefaultPublication())
                .WithRole(PublicationRole.Drafter);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            await contentDbContext.Users.AddAsync(user);
            await contentDbContext.UserPublicationRoles.AddRangeAsync(
                userPublicationRole1,
                userPublicationRole2,
                userPublicationRole3
            );
            await contentDbContext.SaveChangesAsync();

            var service = SetupUserRoleService(contentDbContext: contentDbContext);

            var result = await service.GetPublicationRolesForUser(user.Id);

            result.AssertRight();

            var userPublicationRoles = result.Right;
            Assert.Empty(userPublicationRoles);
        }

        [Fact]
        public async Task GetPublicationRolesForUser_NoUser()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupUserRoleService(contentDbContext: contentDbContext);

            var result = await service.GetPublicationRolesForUser(Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    public class GetPublicationRolesForPublicationTests : UserRoleServiceTests
    {
        [Fact]
        public async Task GetPublicationRolesForPublication()
        {
            User user1 = _dataFixture
                .DefaultUser()
                .WithFirstName("User")
                .WithLastName("1")
                .WithEmail("user1@example.com");

            User user2 = _dataFixture
                .DefaultUser()
                .WithFirstName("User")
                .WithLastName("2")
                .WithEmail("user2@example.com");

            var publication = new Publication { Id = Guid.NewGuid(), Title = "Test Publication 1" };

            var userPublicationRole1 = new UserPublicationRole
            {
                User = user2,
                Publication = publication,
                Role = PublicationRole.Owner,
            };

            var userPublicationRole2 = new UserPublicationRole
            {
                User = user1,
                Publication = publication,
                Role = PublicationRole.Owner,
            };

            // Role assignment for a different publication
            var userPublicationRole3 = new UserPublicationRole
            {
                User = user1,
                Publication = new Publication(),
                Role = PublicationRole.Owner,
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddRangeAsync(user1, user2);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    userPublicationRole1,
                    userPublicationRole2,
                    userPublicationRole3
                );
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
                Assert.Equal(PublicationRole.Owner, userPublicationRoles[0].Role);
                Assert.Equal("user1@example.com", userPublicationRoles[0].Email);

                Assert.Equal(userPublicationRole1.Id, userPublicationRoles[1].Id);
                Assert.Equal("Test Publication 1", userPublicationRoles[1].Publication);
                Assert.Equal("User 2", userPublicationRoles[1].UserName);
                Assert.Equal(PublicationRole.Owner, userPublicationRoles[1].Role);
                Assert.Equal("user2@example.com", userPublicationRoles[1].Email);
            }
        }

        // This test will be changed when we start introducing the use of the NEW publication roles in the
        // UI, in STEP 9 (EES-6196) of the Permissions Rework. For now, we want to
        // filter out any usage of the NEW roles.
        [Fact]
        public async Task GetPublicationRolesForPublication_InvalidRolesNotReturned()
        {
            User user1 = _dataFixture
                .DefaultUser()
                .WithFirstName("User")
                .WithLastName("1")
                .WithEmail("user1@example.com");

            User user2 = _dataFixture
                .DefaultUser()
                .WithFirstName("User")
                .WithLastName("2")
                .WithEmail("user2@example.com");

            var publication1 = _dataFixture.DefaultPublication().Generate();
            var publication2 = _dataFixture.DefaultPublication().Generate();

            var userPublicationRole1 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication1)
                .WithRole(PublicationRole.Approver);

            var userPublicationRole2 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user2)
                .WithPublication(publication1)
                .WithRole(PublicationRole.Approver);

            // Role assignment for a different publication
            var userPublicationRole3 = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user2)
                .WithPublication(publication2)
                .WithRole(PublicationRole.Drafter);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            await contentDbContext.Users.AddRangeAsync(user1, user2);
            await contentDbContext.UserPublicationRoles.AddRangeAsync(
                userPublicationRole1,
                userPublicationRole2,
                userPublicationRole3
            );
            await contentDbContext.SaveChangesAsync();

            var service = SetupUserRoleService(contentDbContext: contentDbContext);

            var result = await service.GetPublicationRolesForPublication(publication1.Id);

            result.AssertRight();

            var userPublicationRoles = result.Right;
            Assert.Empty(userPublicationRoles);
        }

        [Fact]
        public async Task GetPublicationRolesForPublication_NoPublication()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupUserRoleService(contentDbContext: contentDbContext);

            var result = await service.GetPublicationRolesForPublication(Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    public class GetReleaseRolesTests : UserRoleServiceTests
    {
        [Fact]
        public async Task GetReleaseRoles()
        {
            User user = _dataFixture.DefaultUser();

            var (publication1, publication2, publication3) = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)])
                .GenerateTuple3();

            UserReleaseRole userReleaseRole1 = _dataFixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(publication1.Releases[0].Versions[0])
                .WithUser(user)
                .WithRole(ReleaseRole.Contributor);

            UserReleaseRole userReleaseRole2 = _dataFixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(publication2.Releases[0].Versions[0])
                .WithUser(user)
                .WithRole(ReleaseRole.Approver);

            // Role assignment for a different user
            UserReleaseRole userReleaseRole3 = _dataFixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(publication3.Releases[0].Versions[0])
                .WithUser(_dataFixture.DefaultUser().Generate())
                .WithRole(ReleaseRole.Approver);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRole1, userReleaseRole2, userReleaseRole3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserRoleService(contentDbContext: contentDbContext);

                var result = await service.GetReleaseRoles(user.Id);

                var userReleaseRoles = result.AssertRight();

                Assert.Equal(2, userReleaseRoles.Count);

                Assert.Equal(userReleaseRole1.Id, userReleaseRoles[0].Id);
                Assert.Equal(publication1.Title, userReleaseRoles[0].Publication);
                Assert.Equal(publication1.Releases[0].Title, userReleaseRoles[0].Release);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[0].Role);

                Assert.Equal(userReleaseRole2.Id, userReleaseRoles[1].Id);
                Assert.Equal(publication2.Title, userReleaseRoles[1].Publication);
                Assert.Equal(publication2.Releases[0].Title, userReleaseRoles[1].Release);
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
    }

    public class RemoveUserPublicationRoleTests : UserRoleServiceTests
    {
        [Fact]
        public async Task RemoveUserPublicationRole()
        {
            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = PublicationRole.Owner,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst)))
                )
                .ReturnsAsync(new IdentityResult());

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(m => m.Remove(It.Is<UserPublicationRole>(urr => urr.Id == userPublicationRole.Id), default))
                .Returns(Task.CompletedTask);
            userPublicationRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository);
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
            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = PublicationRole.Owner,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the BAU role, which is higher-powered than Analyst, so we will test that they remain
            // a BAU user and do not have any roles removed.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.BauUser));

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(m => m.Remove(It.Is<UserPublicationRole>(urr => urr.Id == userPublicationRole.Id), default))
                .Returns(Task.CompletedTask);
            userPublicationRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userReleaseRoleRepository);
        }

        [Fact]
        public async Task RemoveUserPublicationRole_AnalystRoleStillRequiredForOtherPublications()
        {
            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = PublicationRole.Owner,
            };

            var anotherUserPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = PublicationRole.Owner,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    userPublicationRole,
                    anotherUserPublicationRole
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they retain the Analyst role  because
            // they still have need of it in other Publication roles.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(m => m.Remove(It.Is<UserPublicationRole>(urr => urr.Id == userPublicationRole.Id), default))
                .Returns(Task.CompletedTask);
            userPublicationRoleRepository
                .Setup(m => m.GetDistinctRolesByUser(user.Id))
                .ReturnsAsync([PublicationRole.Owner]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userReleaseRoleRepository);
        }

        [Fact]
        public async Task RemoveUserPublicationRole_AnalystRoleStillRequiredForOtherReleases()
        {
            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = PublicationRole.Owner,
            };

            var userReleaseRole = new UserReleaseRole { User = user, Role = ReleaseRole.Approver };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they retain the Analyst role because
            // they still have need of it in other Release roles.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository
                .Setup(m => m.GetDistinctRolesByUser(user.Id))
                .ReturnsAsync([ReleaseRole.Approver]);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(m => m.Remove(It.Is<UserPublicationRole>(urr => urr.Id == userPublicationRole.Id), default))
                .Returns(Task.CompletedTask);
            userPublicationRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userReleaseRoleRepository);
        }

        [Fact]
        public async Task RemoveUserPublicationRole_DowngradeToPrereleaseRole()
        {
            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var userPublicationRole = new UserPublicationRole
            {
                User = user,
                PublicationId = Guid.NewGuid(),
                Role = PublicationRole.Owner,
            };

            var prereleaseRole = new UserReleaseRole { User = user, Role = ReleaseRole.PrereleaseViewer };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.UserReleaseRoles.AddAsync(prereleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they lose the Analyst role but gain the
            // PrereleaseUser role, as they have need of this role elsewhere.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst)))
                )
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(identityUser), RoleNames.PrereleaseUser))
                .ReturnsAsync(new IdentityResult());

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository
                .Setup(m => m.GetDistinctRolesByUser(user.Id))
                .ReturnsAsync([ReleaseRole.PrereleaseViewer]);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(m => m.Remove(It.Is<UserPublicationRole>(urr => urr.Id == userPublicationRole.Id), default))
                .Returns(Task.CompletedTask);
            userPublicationRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserPublicationRole(userPublicationRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userReleaseRoleRepository);
        }
    }

    public class RemoveUserReleaseRoleTests : UserRoleServiceTests
    {
        [Fact]
        public async Task RemoveUserReleaseRole()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.Approver,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst)))
                )
                .ReturnsAsync(new IdentityResult());

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(m => m.Remove(It.Is<UserReleaseRole>(urr => urr.Id == userReleaseRole.Id), default))
                .Returns(Task.CompletedTask);
            userReleaseRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository);
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
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.Approver,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the BAU role, which is higher-powered than Analyst, so we will test that they remain
            // a BAU user and do not have any roles removed.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.BauUser));

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(m => m.Remove(It.Is<UserReleaseRole>(urr => urr.Id == userReleaseRole.Id), default))
                .Returns(Task.CompletedTask);
            userReleaseRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userReleaseRoleRepository);
        }

        [Fact]
        public async Task RemoveUserReleaseRole_AnalystRoleStillRequiredForOtherReleases()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.Approver,
            };

            var anotherUserReleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Approver,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRole, anotherUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they retain the Analyst role  because
            // they still have need of it in other Release roles.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(m => m.Remove(It.Is<UserReleaseRole>(urr => urr.Id == userReleaseRole.Id), default))
                .Returns(Task.CompletedTask);
            userReleaseRoleRepository
                .Setup(m => m.GetDistinctRolesByUser(user.Id))
                .ReturnsAsync([ReleaseRole.Approver]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userReleaseRoleRepository);
        }

        [Fact]
        public async Task RemoveUserReleaseRole_AnalystRoleStillRequiredForOtherPublications()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            User user = _dataFixture.DefaultUser();

            var analystGlobalRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = Role.Analyst.ToString() };

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var identityUserRole = new IdentityUserRole<string>
            {
                UserId = identityUser.Id,
                RoleId = analystGlobalRole.Id,
            };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.Approver,
            };

            var userPublicationRole = new UserPublicationRole { User = user, Role = PublicationRole.Owner };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.Roles.AddAsync(analystGlobalRole);
                await userAndRolesDbContext.UserRoles.AddAsync(identityUserRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they retain the Analyst role because
            // they still have need of it in other Publication roles.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository
                .Setup(m => m.Remove(It.Is<UserReleaseRole>(urr => urr.Id == userReleaseRole.Id), default))
                .Returns(Task.CompletedTask);
            userReleaseRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(m => m.GetDistinctRolesByUser(user.Id))
                .ReturnsAsync([PublicationRole.Owner]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userReleaseRoleRepository, userPublicationRoleRepository);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userGlobalRoles = await userAndRolesDbContext.UserRoles.ToListAsync();

                var globalRole = Assert.Single(userGlobalRoles);

                Assert.Equal(analystGlobalRole.Id, globalRole.RoleId);
            }
        }

        [Fact]
        public async Task RemoveUserReleaseRole_DowngradeToPrereleaseRole()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            ReleaseVersion anotherReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var userReleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.Approver,
            };

            var prereleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = anotherReleaseVersion,
                Role = ReleaseRole.PrereleaseViewer,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRole, prereleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.Add(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            // Here the user has the Analyst role currently.  We will test that they lose the Analyst role but gain the
            // PrereleaseUser role, as they have need of this role elsewhere.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            userManager
                .Setup(s =>
                    s.RemoveFromRolesAsync(ItIsUser(identityUser), ItIs.ListSequenceEqualTo(ListOf(RoleNames.Analyst)))
                )
                .ReturnsAsync(new IdentityResult());

            userManager
                .Setup(s => s.AddToRoleAsync(ItIsUser(identityUser), RoleNames.PrereleaseUser))
                .ReturnsAsync(new IdentityResult());

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(m => m.Remove(It.Is<UserReleaseRole>(urr => urr.Id == userReleaseRole.Id), default))
                .Returns(Task.CompletedTask);
            userReleaseRoleRepository
                .Setup(m => m.GetDistinctRolesByUser(user.Id))
                .ReturnsAsync([ReleaseRole.PrereleaseViewer]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userReleaseRoleRepository);
        }

        [Fact]
        public async Task RemoveUserReleaseRoleAsPrerelease_AnalystRoleRetained()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            User user = _dataFixture.DefaultUser();

            var identityUser = new ApplicationUser { Id = user.Id.ToString() };

            var approverRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.Approver,
            };

            var prereleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.PrereleaseViewer,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddRangeAsync(approverRole, prereleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                await userAndRolesDbContext.Users.AddAsync(identityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userManager = MockUserManager();

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository.Setup(m => m.GetDistinctRolesByUser(user.Id)).ReturnsAsync([]);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(m => m.Remove(It.Is<UserReleaseRole>(urr => urr.Id == prereleaseRole.Id), default))
                .Returns(Task.CompletedTask);
            userReleaseRoleRepository
                .Setup(m => m.GetDistinctRolesByUser(user.Id))
                .ReturnsAsync([ReleaseRole.Approver]);

            // Here the user has the Analyst role currently but now we are removing a Prerelease Release Role.
            // We will test that they retain the Analyst role.
            userManager.Setup(s => s.GetRolesAsync(ItIsUser(identityUser))).ReturnsAsync(ListOf(RoleNames.Analyst));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserReleaseRole(prereleaseRole.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager, userPublicationRoleRepository, userReleaseRoleRepository);
        }
    }

    public class RemoveAllUserResourceRolesTests : UserRoleServiceTests
    {
        [Fact]
        public async Task RemoveAllUserResourceRoles()
        {
            User targetUser = _dataFixture.DefaultUser().WithEmail("test@test.com");
            var targetIdentityUser = new ApplicationUser { Id = targetUser.Id.ToString() };

            User otherUser = _dataFixture.DefaultUser().WithEmail("otherTestUser@test.com");
            var otherIdentityUser = new ApplicationUser { Id = otherUser.Id.ToString() };

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

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(targetUser, otherUser);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                userAndRolesDbContext.Users.AddRange(targetIdentityUser, otherIdentityUser);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository.Setup(m => m.RemoveForUser(targetUser.Id, default)).Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            userPublicationRoleRepository
                .Setup(m => m.RemoveForUser(targetUser.Id, default))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupUserRoleService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userManager: userManager.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.RemoveAllUserResourceRoles(targetUser.Id);

                result.AssertRight();
            }

            VerifyAllMocks(userManager);
        }
    }

    private static ApplicationUser ItIsUser(ApplicationUser user)
    {
        return It.Is<ApplicationUser>(applicationUser => applicationUser.Id == user.Id);
    }

    private UserRoleService SetupUserRoleService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        UserManager<ApplicationUser>? userManager = null,
        IUserService? userService = null
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
            releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            userManager ?? MockUserManager().Object
        );
    }
}
