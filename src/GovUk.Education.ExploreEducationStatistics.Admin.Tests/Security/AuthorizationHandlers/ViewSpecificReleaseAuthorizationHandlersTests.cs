#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseVersionAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class ViewSpecificReleaseAuthorizationHandlersTests
{
    private static readonly DataFixture _fixture = new();
    private static readonly ReleaseVersion _releaseVersion = _fixture
        .DefaultReleaseVersion()
        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

    public class ClaimsTests
    {
        [Fact]
        public async Task HasAccessAllReleasesClaim()
        {
            // Assert that any users with the "AccessAllReleases" claim can view an arbitrary Release
            // (and no other claim allows this)
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, ViewReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Attach(_releaseVersion);
                    return CreateHandler(contentDbContext);
                },
                _releaseVersion,
                claimsExpectedToSucceed: AccessAllReleases
            );
        }
    }

    public class PublicationRoleTests
    {
        [Fact]
        public async Task HasOwnerOrApproverRoleOnParentPublication()
        {
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<ViewReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Attach(_releaseVersion);
                    return CreateHandler(contentDbContext);
                },
                _releaseVersion,
                rolesExpectedToSucceed: [PublicationRole.Owner, PublicationRole.Allower]
            );
        }
    }

    public class ReleaseRoleTests
    {
        [Fact]
        public async Task UnrestrictedViewerRoleOnRelease()
        {
            // Assert that a User who has any unrestricted viewer role on a Release can view the Release
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<ViewReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Attach(_releaseVersion);
                    return CreateHandler(contentDbContext);
                },
                _releaseVersion,
                rolesExpectedToSucceed: [ReleaseRole.Contributor, ReleaseRole.Approver]
            );
        }

        [Fact]
        public async Task PreReleaseUser_WithinPreReleaseAccessWindow()
        {
            User user = _fixture.DefaultUser();

            var successScenario = new ReleaseVersionHandlerTestScenario
            {
                Entity = _releaseVersion,
                User = _fixture.AuthenticatedUser(userId: user.Id),
                UserReleaseRoles =
                [
                    _fixture
                        .DefaultUserReleaseRole()
                        .WithReleaseVersion(_releaseVersion)
                        .WithUser(user)
                        .WithRole(ReleaseRole.PrereleaseViewer),
                ],
                ExpectedToPass = true,
                UnexpectedFailMessage = "Expected the test to succeed because the Pre Release window is currently open",
            };

            var preReleaseService = new Mock<IPreReleaseService>(Strict);

            preReleaseService
                .Setup(s => s.GetPreReleaseWindowStatus(_releaseVersion, It.IsAny<DateTimeOffset>()))
                .Returns(new PreReleaseWindowStatus { Access = PreReleaseAccess.Within });

            // Assert that a User who specifically has the Pre Release role will cause this handler to succeed
            // if the Pre Release window is currently open.
            await AssertReleaseVersionHandlerHandlesScenarioSuccessfully<ViewReleaseRequirement>(
                contentDbContext => CreateHandler(contentDbContext, preReleaseService.Object),
                successScenario
            );

            VerifyAllMocks(preReleaseService);
        }

        [Fact]
        public async Task PreReleaseUser_OutsidePreReleaseAccessWindow()
        {
            User user = _fixture.DefaultUser();

            var failureScenario = new ReleaseVersionHandlerTestScenario
            {
                Entity = _releaseVersion,
                User = _fixture.AuthenticatedUser(userId: user.Id),
                UserReleaseRoles =
                [
                    _fixture
                        .DefaultUserReleaseRole()
                        .WithReleaseVersion(_releaseVersion)
                        .WithUser(user)
                        .WithRole(ReleaseRole.PrereleaseViewer),
                ],
                ExpectedToPass = false,
                UnexpectedPassMessage =
                    "Expected the test to fail because the Pre Release window is not open at the " + "current time",
            };

            await GetEnums<PreReleaseAccess>()
                .Where(value => value != PreReleaseAccess.Within)
                .ToList()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async access =>
                {
                    var preReleaseService = new Mock<IPreReleaseService>(Strict);

                    preReleaseService
                        .Setup(s => s.GetPreReleaseWindowStatus(_releaseVersion, It.IsAny<DateTimeOffset>()))
                        .Returns(new PreReleaseWindowStatus { Access = access });

                    // Assert that a User who specifically has the Pre Release role will cause this handler to fail
                    // IF the Pre Release window is NOT open
                    await AssertReleaseVersionHandlerHandlesScenarioSuccessfully<ViewReleaseRequirement>(
                        contentDbContext =>
                        {
                            contentDbContext.Attach(_releaseVersion);
                            return CreateHandler(contentDbContext, preReleaseService.Object);
                        },
                        failureScenario
                    );

                    VerifyAllMocks(preReleaseService);
                });
        }
    }

    private static ViewSpecificReleaseAuthorizationHandler CreateHandler(
        ContentDbContext contentDbContext,
        IPreReleaseService? preReleaseService = null
    )
    {
        return new ViewSpecificReleaseAuthorizationHandler(
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: new UserReleaseRoleRepository(contentDbContext),
                userPublicationRoleRepository: new UserPublicationRoleRepository(contentDbContext),
                preReleaseService: preReleaseService
                    ?? new PreReleaseService(
                        new PreReleaseAccessOptions
                        {
                            AccessWindow = new AccessWindowOptions { MinutesBeforeReleaseTimeStart = 200 },
                        }.ToOptionsWrapper()
                    )
            )
        );
    }
}
