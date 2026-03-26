#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class ViewSpecificReleaseAuthorizationHandlersTests
{
    private static readonly DataFixture _dataFixture = new();

    private static readonly ReleaseVersion _releaseVersion = _dataFixture
        .DefaultReleaseVersion()
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

    [Fact]
    public async Task SucceedsIfReleaseVersionIsViewableByUser()
    {
        await AssertHandlerSucceedsIfReleaseVersionIsViewableByUser<ViewReleaseRequirement, ReleaseVersion>(
            handlerSupplier: SetupHandler,
            entity: _releaseVersion,
            releaseVersion: _releaseVersion
        );
    }

    private static ViewSpecificReleaseAuthorizationHandler SetupHandler(
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(authorizationHandlerService);
    }

    private static IAuthorizationHandlerService CreateDefaultAuthorizationHandlerService()
    {
        var mock = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
        mock.Setup(s => s.IsReleaseVersionViewableByUser(_releaseVersion, It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(false);

        return mock.Object;
    }
}
