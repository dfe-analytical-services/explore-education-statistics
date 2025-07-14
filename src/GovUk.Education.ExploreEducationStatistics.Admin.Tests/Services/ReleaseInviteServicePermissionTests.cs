#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseInviteServicePermissionTest
    {
        [Fact]
        public async Task InviteContributor()
        {
            var releaseVersion = new ReleaseVersion();
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                ReleaseVersions = ListOf(releaseVersion),
            };

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                    tuple => tuple.Item1.Id == publication.Id && tuple.Item2 == ReleaseRole.Contributor,
                    CanUpdateSpecificReleaseRole)
                .AssertForbidden(async userService =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        await contentDbContext.AddAsync(publication);
                        await contentDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        var service = SetupReleaseInviteService(
                            contentDbContext: contentDbContext,
                            userService: userService.Object);
                        return await service.InviteContributor("test@test.com",
                            publication.Id,
                            ListOf(releaseVersion.Id));
                    }
                });
        }

        [Fact]
        public async Task RemoveByPublication()
        {
            var releaseVersion = new ReleaseVersion();
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                ReleaseVersions = ListOf(releaseVersion),
            };

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                    tuple => tuple.Item1.Id == publication.Id && tuple.Item2 == ReleaseRole.Contributor,
                    CanUpdateSpecificReleaseRole)
                .AssertForbidden(async userService =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        await contentDbContext.AddAsync(publication);
                        await contentDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        var service = SetupReleaseInviteService(
                            contentDbContext: contentDbContext,
                            userService: userService.Object);
                        return await service.RemoveByPublication("test@test.com",
                            publication.Id,
                            ReleaseRole.Contributor);
                    }
                });
        }

        private static ReleaseInviteService SetupReleaseInviteService(
            ContentDbContext? contentDbContext = null,
            UsersAndRolesDbContext? usersAndRolesDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IReleaseVersionRepository? releaseVersionRepository = null,
            IUserRepository? userRepository = null,
            IUserService? userService = null,
            IUserRoleService? userRoleService = null,
            IUserInviteRepository? userInviteRepository = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null,
            IUserReleaseRoleAndInviteManager? userReleaseRoleRepository = null,
            IEmailService? emailService = null,
            IOptions<AppOptions>? appOptions = null,
            IOptions<NotifyOptions>? notifyOptions = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new ReleaseInviteService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext),
                userRepository ?? new UserRepository(contentDbContext),
                userService ?? AlwaysTrueUserService().Object,
                userRoleService ?? Mock.Of<IUserRoleService>(Strict),
                userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
                userReleaseInviteRepository ?? new UserReleaseInviteRepository(contentDbContext),
                userReleaseRoleRepository ?? new UserReleaseRoleManager(contentDbContext),
                emailService ?? Mock.Of<IEmailService>(Strict),
                appOptions ?? Mock.Of<IOptions<AppOptions>>(),
                notifyOptions ?? Mock.Of<IOptions<NotifyOptions>>()
            );
        }
    }
}
