#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.PublicationAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class PublicationReleaseSeriesAuthorizationHandlersTests
{
    public class ManagePublicationReleaseSeriesAuthorizationHandlerTests
    {
        [Fact]
        public async Task ManagePublicationReleaseSeries_Claims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<Publication, ManagePublicationReleaseSeriesRequirement>(
                CreateHandler,
                new Publication(),
                UpdateAllPublications
            );
        }

        [Fact]
        public async Task ManagePublicationReleaseSeries_PublicationRoles()
        {
            await AssertPublicationHandlerSucceedsWithPublicationRoles<ManagePublicationReleaseSeriesRequirement>(
                CreateHandler,
                Owner
            );
        }

        private static ManagePublicationReleaseSeriesAuthorizationHandler CreateHandler(
            ContentDbContext contentDbContext
        )
        {
            var userReleaseRoleRepository = new UserReleaseRoleRepository(
                contentDbContext,
                logger: Mock.Of<ILogger<UserReleaseRoleRepository>>()
            );

            var userPublicationRoleRepository = new UserPublicationRoleRepository(contentDbContext);

            return new ManagePublicationReleaseSeriesAuthorizationHandler(
                new AuthorizationHandlerService(
                    releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                    userReleaseRoleRepository: userReleaseRoleRepository,
                    userPublicationRoleRepository: userPublicationRoleRepository,
                    preReleaseService: Mock.Of<IPreReleaseService>(Strict)
                )
            );
        }
    }
}
