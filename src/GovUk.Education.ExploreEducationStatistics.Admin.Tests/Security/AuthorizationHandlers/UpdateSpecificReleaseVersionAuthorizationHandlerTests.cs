#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseVersionAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class UpdateSpecificReleaseVersionAuthorizationHandlerTests
{
    private static readonly DataFixture _fixture = new();

    public class ClaimTests
    {
        // Test that Releases Versions that are in any approval state other than Approved can be updated
        // if the current user has the "UpdateAllReleases" Claim.
        [Fact]
        public async Task UpdateAllReleases_ClaimSucceedsIfNotApproved()
        {
            await GetEnums<ReleaseApprovalStatus>()
                .Where(releaseStatus => releaseStatus != Approved)
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async status =>
                {
                    ReleaseVersion releaseVersion = _fixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatus(status)
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

                    await AssertHandlerSucceedsWithCorrectClaims<
                        ReleaseVersion,
                        UpdateSpecificReleaseVersionRequirement
                    >(HandlerSupplier, releaseVersion, SecurityClaimTypes.UpdateAllReleases);
                });
        }

        // Test that Releases Versions that are Approved cannot be updated by a user having any Claim.
        [Fact]
        public async Task AllClaimsFailIfApproved()
        {
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(Approved)
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, UpdateSpecificReleaseVersionRequirement>(
                HandlerSupplier,
                releaseVersion,
                claimsExpectedToSucceed: []
            );
        }
    }

    public class PublicationRoleTests
    {
        [Fact]
        public async Task PublicationOwnerAndApproversCanUpdateUnapprovedReleaseVersion()
        {
            await GetEnums<ReleaseApprovalStatus>()
                .Where(releaseStatus => releaseStatus != Approved)
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async status =>
                {
                    ReleaseVersion releaseVersion = _fixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatus(status)
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

                    // Assert that a Publication Owner or Approver can update the Release Version in any approval
                    // state other than Admin
                    await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<UpdateSpecificReleaseVersionRequirement>(
                        HandlerSupplier,
                        releaseVersion,
                        rolesExpectedToSucceed: [PublicationRole.Owner, PublicationRole.Allower]
                    );
                });
        }

        [Fact]
        public async Task NoRolesCanUpdateApprovedReleaseVersion()
        {
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(Approved)
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            // Assert that no Publication Role can update the Release Version if it is Approved.
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<UpdateSpecificReleaseVersionRequirement>(
                HandlerSupplier,
                releaseVersion,
                rolesExpectedToSucceed: []
            );
        }
    }

    public class ReleaseRoleTests
    {
        [Fact]
        public async Task EditorsCanUpdateUnapprovedReleaseVersion()
        {
            await GetEnums<ReleaseApprovalStatus>()
                .Where(releaseStatus => releaseStatus != Approved)
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async status =>
                {
                    ReleaseVersion releaseVersion = _fixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatus(status)
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

                    // Assert that a Release Editor (Contributor, Approver) can update the Release
                    // Version in any approval state other than Approved.
                    await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<UpdateSpecificReleaseVersionRequirement>(
                        HandlerSupplier,
                        releaseVersion,
                        rolesExpectedToSucceed: [ReleaseRole.Contributor, ReleaseRole.Approver]
                    );
                });
        }

        [Fact]
        public async Task NoRolesCanUpdateApprovedReleaseVersion()
        {
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(Approved)
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            // Assert that no Publication Role can update the Release Version if it is Approved.
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<UpdateSpecificReleaseVersionRequirement>(
                HandlerSupplier,
                releaseVersion,
                rolesExpectedToSucceed: []
            );
        }
    }

    private static UpdateSpecificReleaseVersionAuthorizationHandler HandlerSupplier(ContentDbContext contentDbContext)
    {
        return new UpdateSpecificReleaseVersionAuthorizationHandler(
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: new UserReleaseRoleRepository(contentDbContext: contentDbContext),
                userPublicationRoleRepository: new UserPublicationRoleRepository(contentDbContext: contentDbContext),
                preReleaseService: Mock.Of<IPreReleaseService>(Strict)
            )
        );
    }
}
