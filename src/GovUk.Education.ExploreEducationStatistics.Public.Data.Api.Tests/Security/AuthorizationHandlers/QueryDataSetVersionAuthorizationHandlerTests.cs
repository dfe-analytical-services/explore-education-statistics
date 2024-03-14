using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Security.AuthorizationHandlers;

public class QueryDataSetVersionAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();

    [Theory]
    [InlineData(DataSetVersionStatus.Deprecated)]
    [InlineData(DataSetVersionStatus.Published)]
    public void Success(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var handler = BuildHandler();
        var context = CreateAnonymousAuthContext<QueryDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData(DataSetVersionStatus.Processing)]
    [InlineData(DataSetVersionStatus.Failed)]
    [InlineData(DataSetVersionStatus.Mapping)]
    [InlineData(DataSetVersionStatus.Draft)]
    [InlineData(DataSetVersionStatus.Withdrawn)]
    public void Failure(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var handler = BuildHandler();
        var context = CreateAnonymousAuthContext<QueryDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static QueryDataSetVersionAuthorizationHandler BuildHandler()
    {
        return new QueryDataSetVersionAuthorizationHandler();
    }
}
