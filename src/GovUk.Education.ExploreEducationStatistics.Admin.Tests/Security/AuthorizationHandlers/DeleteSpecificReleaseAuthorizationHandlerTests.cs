#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseVersionAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class DeleteSpecificReleaseAuthorizationHandlerTests
{
    private static readonly DataFixture _dataFixture = new();

    public class ClaimsTests
    {
        [Fact]
        public async Task DeleteSpecificReleaseAuthorizationHandler_NotAmendment()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithVersion(0)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that no users can delete the first version of a release
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, DeleteSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion
            );
        }

        [Fact]
        public async Task DeleteSpecificReleaseAuthorizationHandler_AmendmentButApproved()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithVersion(1)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that no users can delete an amendment release version that is approved
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, DeleteSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion
            );
        }

        [Fact]
        public async Task DeleteSpecificReleaseAuthorizationHandler_UnapprovedAmendment()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithVersion(1)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that users with the "DeleteAllReleaseAmendments" claim can delete an amendment release version that is not yet approved
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, DeleteSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion,
                DeleteAllReleaseAmendments
            );
        }
    }

    public class PublicationRoleTests
    {
        [Fact]
        public async Task DeleteSpecificReleaseAuthorizationHandler_NotAmendment()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithVersion(0)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that no User Publication roles will allow deleting the first version of a release
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<DeleteSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion
            );
        }

        [Fact]
        public async Task DeleteSpecificReleaseAuthorizationHandler_AmendmentButApproved()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithVersion(1)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that no User Publication roles will allow deleting an amendment release version when it is Approved
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<DeleteSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion
            );
        }

        [Fact]
        public async Task DeleteSpecificReleaseAuthorizationHandler_UnapprovedAmendment()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithVersion(1)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that users with the Publication Owner role on an amendment release version can delete if it is not yet approved
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<DeleteSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion,
                Owner
            );
        }
    }

    public class ReleaseRoleTests
    {
        [Fact]
        public async Task DeleteSpecificReleaseAuthorizationHandler_NotAmendment()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithVersion(0)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that no User Release roles will allow deleting the first version of a release
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion
            );
        }

        [Fact]
        public async Task DeleteSpecificReleaseAuthorizationHandler_AmendmentButApproved()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithVersion(1)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that no User Release roles will allow deleting an amendment release version when it is Approved
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion
            );
        }

        [Fact]
        public async Task DeleteSpecificReleaseAuthorizationHandler_UnapprovedAmendment()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithVersion(1)
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            // Assert that no User Release roles will allow an amendment release version to be deleted if it is not yet approved
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                CreateHandler,
                releaseVersion
            );
        }
    }

    private static DeleteSpecificReleaseAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        var userReleaseRoleRepository = new UserReleaseRoleRepository(contentDbContext);

        var userPublicationRoleRepository = new UserPublicationRoleRepository(contentDbContext);

        return new DeleteSpecificReleaseAuthorizationHandler(
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: userReleaseRoleRepository,
                userPublicationRoleRepository: userPublicationRoleRepository,
                preReleaseService: Mock.Of<IPreReleaseService>(Strict)
            )
        );
    }
}
