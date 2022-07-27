#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.Tests.AuthorizationHandlers;

public class ViewMethodologyVersionAuthorizationHandlerTests
{
    private readonly MethodologyVersion _methodologyVersion = new()
    {
        Id = Guid.NewGuid()
    };

    [Fact]
    public async Task MethodologyVersionIsPubliclyAccessible()
    {
        var (
            handler,
            methodologyVersionRepository
            ) = CreateHandlerAndDependencies();

        methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(_methodologyVersion.Id))
            .ReturnsAsync(true);

        var authContext =
            CreateAnonymousAuthContext<ViewMethodologyVersionRequirement, MethodologyVersion>(_methodologyVersion);

        await handler.HandleAsync(authContext);

        VerifyAllMocks(methodologyVersionRepository);

        Assert.True(authContext.HasSucceeded);
    }

    [Fact]
    public async Task MethodologyVersionIsNotPubliclyAccessible()
    {
        var (
            handler,
            methodologyVersionRepository
            ) = CreateHandlerAndDependencies();

        methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(_methodologyVersion.Id))
            .ReturnsAsync(false);

        var authContext =
            CreateAnonymousAuthContext<ViewMethodologyVersionRequirement, MethodologyVersion>(_methodologyVersion);

        await handler.HandleAsync(authContext);

        VerifyAllMocks(methodologyVersionRepository);

        Assert.False(authContext.HasSucceeded);
    }

    private static
        (ViewMethodologyVersionAuthorizationHandler,
        Mock<IMethodologyVersionRepository>
        )
        CreateHandlerAndDependencies()
    {
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

        var handler = new ViewMethodologyVersionAuthorizationHandler(
            methodologyVersionRepository.Object
        );

        return (
            handler,
            methodologyVersionRepository
        );
    }
}
