#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseVersionAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class PublishSpecificReleaseAuthorizationHandlerTests
{
    public class ClaimTests
    {
        [Fact]
        public async Task FailsWhenDraft()
        {
            // Assert that no claims will allow a draft release version to be published
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, PublishSpecificReleaseRequirement>(
                CreateHandler,
                new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    ApprovalStatus = Draft
                }
            );
        }

        [Fact]
        public async Task SucceedsWhenApproved()
        {
            // Assert that the PublishAllReleases claim will allow an approved release version to be published
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, PublishSpecificReleaseRequirement>(
                CreateHandler,
                new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    ApprovalStatus = Approved
                },
                PublishAllReleases
            );
        }
    }

    public class ReleaseRoleTests
    {
        [Fact]
        public async Task FailsWhenDraft()
        {
            // Assert that no User Release roles will allow a draft release version to be published
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<PublishSpecificReleaseRequirement>(
                CreateHandler,
                new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    ApprovalStatus = Draft
                }
            );
        }

        [Fact]
        public async Task SucceedsWhenApproved()
        {
            // Assert that only the Approver User Release role will allow an approved release version to be published
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<PublishSpecificReleaseRequirement>(
                CreateHandler,
                new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    ApprovalStatus = Approved
                },
                Approver);
        }
    }

    private static PublishSpecificReleaseAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        return new PublishSpecificReleaseAuthorizationHandler(
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(contentDbContext),
                new UserReleaseRoleRepository(contentDbContext),
                new UserPublicationRoleRepository(contentDbContext),
                Mock.Of<IPreReleaseService>(Strict)));
    }
}
