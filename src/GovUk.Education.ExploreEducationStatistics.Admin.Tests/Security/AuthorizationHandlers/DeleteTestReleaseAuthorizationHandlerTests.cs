#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public abstract class DeleteTestReleaseAuthorizationHandlerTests
{
    public class HandleRequirementAsyncTests : DeleteTestReleaseAuthorizationHandlerTests
    {
        private static readonly ReleaseVersion TestReleaseVersion = new()
        {
            ApprovalStatus = ReleaseApprovalStatus.Draft,
            Version = 1,
            Publication = new Publication { Topic = new Topic { Title = "UI test topic" } }
        };

        private static readonly ReleaseVersion NonTestReleaseVersion = new()
        {
            ApprovalStatus = ReleaseApprovalStatus.Draft,
            Version = 1,
            Publication = new Publication { Topic = new Topic { Title = "Normal topic" } }
        };

        [Fact]
        public async Task Success()
        {
            await AssertHandlerSucceedsWithCorrectGlobalRoles<ReleaseVersion, DeleteTestReleaseRequirement>(
                _ => CreateHandler(enableThemeDeletion: true),
                TestReleaseVersion,
                rolesExpectedToSucceed: [GlobalRoles.Role.BauUser]);
        }

        [Fact]
        public async Task NonTestReleaseVersion_Forbidden()
        {
            await AssertHandlerSucceedsWithCorrectGlobalRoles<ReleaseVersion, DeleteTestReleaseRequirement>(
                _ => CreateHandler(enableThemeDeletion: true),
                NonTestReleaseVersion);
        }

        [Fact]
        public async Task ThemeDeletionEnabledIsFalse_Forbidden()
        {
            await AssertHandlerSucceedsWithCorrectGlobalRoles<ReleaseVersion, DeleteTestReleaseRequirement>(
                _ => CreateHandler(enableThemeDeletion: false),
                TestReleaseVersion);
        }
    }

    private static DeleteTestReleaseAuthorizationHandler CreateHandler(
        bool enableThemeDeletion = false)
    {
        return new DeleteTestReleaseAuthorizationHandler(
            appOptions: new AppOptions { EnableThemeDeletion = enableThemeDeletion }.ToOptionsWrapper());
    }
}
