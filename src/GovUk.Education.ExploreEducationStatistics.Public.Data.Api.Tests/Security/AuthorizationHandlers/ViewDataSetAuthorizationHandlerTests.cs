using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Security.AuthorizationHandlers;

public class ViewDataSetAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();

    [Theory]
    [InlineData(DataSetStatus.Deprecated)]
    [InlineData(DataSetStatus.Published)]
    public void Success(DataSetStatus status)
    {
        DataSet dataSet = _dataFixture
            .DefaultDataSet()
            .WithStatus(status);

        var handler = BuildHandler();
        var context = CreateAnonymousAuthContext<ViewDataSetRequirement, DataSet>(dataSet);

        handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData(DataSetStatus.Draft)]
    [InlineData(DataSetStatus.Withdrawn)]
    public void Failure(DataSetStatus status)
    {
        DataSet dataSet = _dataFixture
            .DefaultDataSet()
            .WithStatus(status);

        var handler = BuildHandler();
        var context = CreateAnonymousAuthContext<ViewDataSetRequirement, DataSet>(dataSet);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static ViewDataSetAuthorizationHandler BuildHandler()
    {
        return new ViewDataSetAuthorizationHandler();
    }
}
