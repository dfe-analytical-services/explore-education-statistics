using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Security.AuthorizationHandlers;

public class ViewDataSetVersionAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();

    [Theory]
    [InlineData(DataSetVersionStatus.Deprecated)]
    [InlineData(DataSetVersionStatus.Published)]
    [InlineData(DataSetVersionStatus.Withdrawn)]
    public void Success(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var handler = BuildHandler();
        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData(DataSetVersionStatus.Processing)]
    [InlineData(DataSetVersionStatus.Failed)]
    [InlineData(DataSetVersionStatus.Mapping)]
    [InlineData(DataSetVersionStatus.Draft)]
    public void Failure(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var handler = BuildHandler();
        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static ViewDataSetVersionAuthorizationHandler BuildHandler()
    {
        // TODO: EES-5374 - Remove when authentication is added for admin API
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        return new ViewDataSetVersionAuthorizationHandler(httpContextAccessor);
    }
}
