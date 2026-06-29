#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public abstract class DeleteTestReleaseAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly ReleaseVersion _testReleaseVersion;
    private readonly ReleaseVersion _seedReleaseVersion;
    private readonly ReleaseVersion _realReleaseVersion;

    protected DeleteTestReleaseAuthorizationHandlerTests()
    {
        _testReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithVersion(1)
            .WithRelease(
                _dataFixture
                    .DefaultRelease()
                    .WithPublication(
                        _dataFixture
                            .DefaultPublication()
                            .WithTheme(_dataFixture.DefaultTheme().WithTitle("UI test theme"))
                    )
            );

        _seedReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithVersion(1)
            .WithRelease(
                _dataFixture
                    .DefaultRelease()
                    .WithPublication(
                        _dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme().WithTitle("Seed theme"))
                    )
            );

        _realReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithVersion(1)
            .WithRelease(
                _dataFixture
                    .DefaultRelease()
                    .WithPublication(
                        _dataFixture
                            .DefaultPublication()
                            .WithTheme(_dataFixture.DefaultTheme().WithTitle("Normal theme\""))
                    )
            );
    }

    public class GlobalRolesTests : DeleteTestReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task TestRelease_SucceedsOnlyForValidGlobalRoles()
        {
            await AssertHandlerSucceedsWithCorrectGlobalRoles<DeleteTestReleaseRequirement, ReleaseVersion>(
                BuildHandler(enableThemeDeletion: true),
                _testReleaseVersion,
                rolesExpectedToSucceed: [GlobalRoles.Role.BauUser]
            );
        }

        [Fact]
        public async Task SeedRelease_SucceedsOnlyForValidGlobalRoles()
        {
            await AssertHandlerSucceedsWithCorrectGlobalRoles<DeleteTestReleaseRequirement, ReleaseVersion>(
                BuildHandler(enableThemeDeletion: true),
                _seedReleaseVersion,
                rolesExpectedToSucceed: [GlobalRoles.Role.BauUser]
            );
        }

        [Fact]
        public async Task RealReleaseVersion_FailsForAllGlobalRoles()
        {
            await AssertHandlerFailsForAllGlobalRoles<DeleteTestReleaseRequirement, ReleaseVersion>(
                BuildHandler(enableThemeDeletion: true),
                _realReleaseVersion
            );
        }

        [Fact]
        public async Task ThemeDeletionEnabledIsFalse_FailsForAllGlobalRoles()
        {
            await AssertHandlerFailsForAllGlobalRoles<DeleteTestReleaseRequirement, ReleaseVersion>(
                BuildHandler(enableThemeDeletion: false),
                _testReleaseVersion
            );
        }
    }

    private static DeleteTestReleaseAuthorizationHandler BuildHandler(bool enableThemeDeletion = false) =>
        new DeleteTestReleaseAuthorizationHandler(
            appOptions: new AppOptions { EnableThemeDeletion = enableThemeDeletion }.ToOptionsWrapper()
        );
}
