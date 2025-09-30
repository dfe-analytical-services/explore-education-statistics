#nullable enable
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
            ApprovalStatus = ReleaseApprovalStatus.Approved,
            Version = 1,
            Publication = new Publication { Theme = new Theme { Title = "UI test theme" } }
        };

        private static readonly ReleaseVersion SeedReleaseVersion = new()
        {
            ApprovalStatus = ReleaseApprovalStatus.Approved,
            Version = 1,
            Publication = new Publication { Theme = new Theme { Title = "Seed theme" } }
        };

        private static readonly ReleaseVersion RealReleaseVersion = new()
        {
            ApprovalStatus = ReleaseApprovalStatus.Approved,
            Version = 1,
            Publication = new Publication { Theme = new Theme { Title = "Normal theme" } }
        };

        [Fact]
        public async Task Success_TestRelease()
        {
            await AssertHandlerSucceedsWithCorrectGlobalRoles<ReleaseVersion, DeleteTestReleaseRequirement>(
                _ => CreateHandler(enableThemeDeletion: true),
                TestReleaseVersion,
                rolesExpectedToSucceed: [GlobalRoles.Role.BauUser]);
        }

        [Fact]
        public async Task Success_SeedRelease()
        {
            await AssertHandlerSucceedsWithCorrectGlobalRoles<ReleaseVersion, DeleteTestReleaseRequirement>(
                _ => CreateHandler(enableThemeDeletion: true),
                SeedReleaseVersion,
                rolesExpectedToSucceed: [GlobalRoles.Role.BauUser]);
        }

        [Fact]
        public async Task RealReleaseVersion_Forbidden()
        {
            await AssertHandlerSucceedsWithCorrectGlobalRoles<ReleaseVersion, DeleteTestReleaseRequirement>(
                _ => CreateHandler(enableThemeDeletion: true),
                RealReleaseVersion);
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
