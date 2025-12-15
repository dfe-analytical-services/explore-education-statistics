#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Authorization;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseVersionAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandlerTests
{
    private static readonly DataFixture _dataFixture = new();

    public class ClaimsTests
    {
        [Fact]
        public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_NotApproved()
        {
            // Assert that users with the "UpdateAllReleases" claim can assign pre release contacts to a release
            // that's unapproved
            await AssertHandlerSucceedsWithCorrectClaims<
                ReleaseVersion,
                AssignPrereleaseContactsToSpecificReleaseRequirement
            >(CreateHandler, new ReleaseVersion { ApprovalStatus = ReleaseApprovalStatus.Draft }, UpdateAllReleases);
        }

        [Fact]
        public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_Approved()
        {
            // Assert that users with the "UpdateAllReleases" claim can assign pre release contacts to a release
            // that's approved
            await AssertHandlerSucceedsWithCorrectClaims<
                ReleaseVersion,
                AssignPrereleaseContactsToSpecificReleaseRequirement
            >(CreateHandler, new ReleaseVersion { ApprovalStatus = ReleaseApprovalStatus.Approved }, UpdateAllReleases);
        }
    }

    public class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandlerPublicationRoleTests
    {
        [Fact]
        public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_NotApproved()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that users with the Publication Owner role can assign pre release contacts to a release
            // that's unapproved
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<AssignPrereleaseContactsToSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion,
                PublicationRole.Owner,
                PublicationRole.Allower
            );
        }

        [Fact]
        public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_Approved()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that users with the Publication Owner role can assign pre release contacts to a release
            // that's approved
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<AssignPrereleaseContactsToSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion,
                PublicationRole.Owner,
                PublicationRole.Allower
            );
        }
    }

    public class ReleaseRoleTests
    {
        [Fact]
        public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_NotApproved()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that users with an editor User Release role can assign pre release contacts to a release
            // that's unapproved
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<AssignPrereleaseContactsToSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion,
                ReleaseRole.Approver,
                ReleaseRole.Contributor
            );
        }

        [Fact]
        public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_Approved()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that users with an editor User Release role can assign pre release contacts to a release
            // that's approved
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<AssignPrereleaseContactsToSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion,
                ReleaseRole.Approver,
                ReleaseRole.Contributor
            );
        }
    }

    private static IAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        var userReleaseRoleRepository = new UserReleaseRoleRepository(contentDbContext);
        var userPublicationRoleRepository = new UserPublicationRoleRepository(contentDbContext);

        return new AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: userReleaseRoleRepository,
                userPublicationRoleRepository: userPublicationRoleRepository,
                preReleaseService: Mock.Of<IPreReleaseService>(Strict)
            )
        );
    }
}
