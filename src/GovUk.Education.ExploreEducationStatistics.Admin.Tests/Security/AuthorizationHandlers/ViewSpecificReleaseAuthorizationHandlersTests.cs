using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.ViewSpecificReleaseAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public async Task CanSeeAllReleasesAuthorizationHandler()
        {
            // Assert that any users with the "AccessAllReleases" claim can view an arbitrary Release
            // (and no other claim allows this)
            await AssertReleaseHandlerSucceedsWithCorrectClaims<ViewReleaseRequirement>(
                new CanSeeAllReleasesAuthorizationHandler(), AccessAllReleases);
        }

        [Fact]
        public async Task HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has any unrestricted viewer role on a Release can view the Release
            await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewReleaseRequirement>(
                contentDbContext =>
                    new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(
                        new AuthorizationHandlerResourceRoleService(
                            new UserReleaseRoleRepository(contentDbContext),
                            new UserPublicationRoleRepository(contentDbContext),
                            Mock.Of<IPublicationRepository>(Strict))),
                ReleaseRole.Viewer, ReleaseRole.Lead, ReleaseRole.Contributor, ReleaseRole.Approver);
        }

        [Fact]
        public async Task HasOwnerOrApproverRoleOnParentPublicationAuthorizationHandler()
        {
            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };
            await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<ViewReleaseRequirement>(
                contentDbContext => new HasOwnerOrApproverRoleOnParentPublicationAuthorizationHandler(
                        new AuthorizationHandlerResourceRoleService(
                            Mock.Of<IUserReleaseRoleRepository>(Strict),
                            new UserPublicationRoleRepository(contentDbContext),
                            Mock.Of<IPublicationRepository>(Strict))),
                new Release
                {
                    PublicationId = publication.Id,
                    Publication = publication
                },
                PublicationRole.Owner, PublicationRole.Approver);
        }

        [Fact]
        public async Task HasPreReleaseRoleWithinAccessWindowAuthorizationHandler()
        {
            var release = new Release();

            var preReleaseService = new Mock<IPreReleaseService>();

            preReleaseService
                .Setup(s => s.GetPreReleaseWindowStatus(release, It.IsAny<DateTime>()))
                .Returns(new PreReleaseWindowStatus
                {
                    Access = PreReleaseAccess.Within
                });

            // Assert that a User who specifically has the Pre Release role will cause this handler to pass
            // IF the Pre Release window is open
            await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewReleaseRequirement>(
                contentDbContext =>
                    new HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(
                        preReleaseService.Object,
                        new AuthorizationHandlerResourceRoleService(
                            new UserReleaseRoleRepository(contentDbContext),
                            new UserPublicationRoleRepository(contentDbContext),
                            Mock.Of<IPublicationRepository>(Strict))),
                release,
                ReleaseRole.PrereleaseViewer);
        }

        [Fact]
        public async Task HasPreReleaseRoleWithinAccessWindowAuthorizationHandler_PreReleaseWindowNotOpen()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var preReleaseService = new Mock<IPreReleaseService>();

            var userId = Guid.NewGuid();

            var failureScenario = new ReleaseHandlerTestScenario
            {
                Entity = release,
                User = ClaimsPrincipalUtils.CreateClaimsPrincipal(userId),
                UserReleaseRoles = new List<UserReleaseRole>
                {
                    new UserReleaseRole
                    {
                        ReleaseId = release.Id,
                        UserId = userId,
                        Role = ReleaseRole.PrereleaseViewer
                    }
                },
                ExpectedToPass = false,
                UnexpectedPassMessage = "Expected the test to fail because the Pre Release window is not open at the " +
                                        "current time"
            };

            await GetEnumValues<PreReleaseAccess>()
                .Where(value => value != PreReleaseAccess.Within)
                .ToList()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async access =>
                {
                    preReleaseService
                        .Setup(s => s.GetPreReleaseWindowStatus(release, It.IsAny<DateTime>()))
                        .Returns(new PreReleaseWindowStatus
                        {
                            Access = access
                        });

                    // Assert that a User who specifically has the Pre Release role will cause this handler to fail
                    // IF the Pre Release window is NOT open
                    await AssertReleaseHandlerHandlesScenarioSuccessfully<ViewReleaseRequirement>(
                        contentDbContext =>
                            new HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(
                                preReleaseService.Object,
                                new AuthorizationHandlerResourceRoleService(
                                    new UserReleaseRoleRepository(contentDbContext),
                                    new UserPublicationRoleRepository(contentDbContext),
                                    Mock.Of<IPublicationRepository>(Strict))),
                        failureScenario);
                });
        }
    }
}
