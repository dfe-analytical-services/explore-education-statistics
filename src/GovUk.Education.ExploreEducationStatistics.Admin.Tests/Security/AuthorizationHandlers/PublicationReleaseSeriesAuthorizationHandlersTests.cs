#nullable enable
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
    PublicationAuthorizationHandlersTestUtil;
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
                CreateHandler, Owner);
        }

        private static ManagePublicationReleaseSeriesAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
        {
            return new ManagePublicationReleaseSeriesAuthorizationHandler(
                new AuthorizationHandlerService(
                    new ReleaseVersionRepository(contentDbContext),
                    Mock.Of<IUserReleaseRoleAndInviteManager>(Strict),
                    new UserPublicationRoleManager(contentDbContext),
                    Mock.Of<IPreReleaseService>(Strict)));
        }
    }
}
