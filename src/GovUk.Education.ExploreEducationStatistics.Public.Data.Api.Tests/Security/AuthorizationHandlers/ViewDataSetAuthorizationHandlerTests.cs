using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Requests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Security.AuthorizationHandlers;

public class ViewDataSetAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();

    [Theory]
    [MemberData(nameof(DataSetStatusTheoryData.AvailableStatuses),
        MemberType = typeof(DataSetStatusTheoryData))]
    public async Task DataSetHasAvailableStatus_Success(DataSetStatus status)
    {
        DataSet dataSet = _dataFixture
            .DefaultDataSet()
            .WithStatus(status);

        var handler = BuildHandler();
        var context = CreateAnonymousAuthContext<ViewDataSetRequirement, DataSet>(dataSet);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetStatusTheoryData.UnavailableStatuses),
        MemberType = typeof(DataSetStatusTheoryData))]
    public async Task DataSetHasUnavailableStatus_Failure(DataSetStatus status)
    {
        DataSet dataSet = _dataFixture
            .DefaultDataSet()
            .WithStatus(status);

        var handler = BuildHandler();
        var context = CreateAnonymousAuthContext<ViewDataSetRequirement, DataSet>(dataSet);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetStatusTheoryData.AllStatuses),
        MemberType = typeof(DataSetStatusTheoryData))]
    public async Task PreviewTokenForDraftDataSetVersionActive_Success(DataSetStatus status)
    {
        DataSet dataSet = _dataFixture
            .DefaultDataSet()
            .WithStatus(status);

        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(DataSetVersionStatus.Draft)
            .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken()])
            .FinishWith(dsv => dataSet.LatestDraftVersionId = dsv.Id);

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSets).ReturnsDbSet([dataSet]);
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet([dataSetVersion]);
        publicDataDbContext.SetupGet(dbContext => dbContext.PreviewTokens).ReturnsDbSet(dataSetVersion.PreviewTokens);

        var handler = BuildHandler(
            publicDataDbContext: publicDataDbContext.Object,
            requestHeaders:
            [
                PreviewTokenRequestHeader(dataSetVersion.PreviewTokens[0])
            ]);
        var context = CreateAnonymousAuthContext<ViewDataSetRequirement, DataSet>(dataSet);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    /// <summary>
    /// Despite the Preview Token being used is expired, the DataSet's status itself is
    /// available to the public, and so the auth succeeds.
    /// </summary>
    [Theory]
    [MemberData(nameof(DataSetStatusTheoryData.AvailableStatuses),
        MemberType = typeof(DataSetStatusTheoryData))]
    public async Task PreviewTokenForDraftDataSetVersionExpired_DataSetStatusAvailable_Success(DataSetStatus status)
    {
        DataSet dataSet = _dataFixture
            .DefaultDataSet()
            .WithStatus(status);

        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(DataSetVersionStatus.Draft)
            .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken(expired: true)])
            .FinishWith(dsv => dataSet.LatestDraftVersionId = dsv.Id);

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSets).ReturnsDbSet([dataSet]);
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet([dataSetVersion]);
        publicDataDbContext.SetupGet(dbContext => dbContext.PreviewTokens).ReturnsDbSet(dataSetVersion.PreviewTokens);

        var handler = BuildHandler(
            publicDataDbContext: publicDataDbContext.Object,
            requestHeaders:
            [
                PreviewTokenRequestHeader(dataSetVersion.PreviewTokens[0])
            ]);
        var context = CreateAnonymousAuthContext<ViewDataSetRequirement, DataSet>(dataSet);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    /// <summary>
    /// The Preview Token being used is expired and the DataSet's status is
    /// unavailable to the public, and so the auth fails.
    /// </summary>
    [Theory]
    [MemberData(nameof(DataSetStatusTheoryData.UnavailableStatuses),
        MemberType = typeof(DataSetStatusTheoryData))]
    public async Task PreviewTokenForDraftDataSetVersionExpired_DataSetStatusUnavailable_Failure(DataSetStatus status)
    {
        DataSet dataSet = _dataFixture
            .DefaultDataSet()
            .WithStatus(status);

        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(DataSetVersionStatus.Draft)
            .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken(expired: true)])
            .FinishWith(dsv => dataSet.LatestDraftVersionId = dsv.Id);

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSets).ReturnsDbSet([dataSet]);
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet([dataSetVersion]);
        publicDataDbContext.SetupGet(dbContext => dbContext.PreviewTokens).ReturnsDbSet(dataSetVersion.PreviewTokens);

        var handler = BuildHandler(
            publicDataDbContext: publicDataDbContext.Object,
            requestHeaders:
            [
                PreviewTokenRequestHeader(dataSetVersion.PreviewTokens[0])
            ]);
        var context = CreateAnonymousAuthContext<ViewDataSetRequirement, DataSet>(dataSet);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    /// <summary>
    /// Despite the Preview Token being used is for a non-draft DataSetVersion, the DataSet's
    /// status itself is available to the public, and so the auth succeeds.
    /// </summary>
    [Theory]
    [MemberData(nameof(DataSetStatusTheoryData.AvailableStatuses),
        MemberType = typeof(DataSetStatusTheoryData))]
    public async Task PreviewTokenActiveButForLiveDataSetVersion_DataSetStatusAvailable_Success(DataSetStatus status)
    {
        DataSet dataSet = _dataFixture
            .DefaultDataSet()
            .WithStatus(status);

        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(DataSetVersionStatus.Published)
            .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken()])
            .FinishWith(dsv => dataSet.LatestLiveVersionId = dsv.Id);

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSets).ReturnsDbSet([dataSet]);
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet([dataSetVersion]);
        publicDataDbContext.SetupGet(dbContext => dbContext.PreviewTokens).ReturnsDbSet(dataSetVersion.PreviewTokens);

        var handler = BuildHandler(
            publicDataDbContext: publicDataDbContext.Object,
            requestHeaders:
            [
                PreviewTokenRequestHeader(dataSetVersion.PreviewTokens[0])
            ]);
        var context = CreateAnonymousAuthContext<ViewDataSetRequirement, DataSet>(dataSet);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    /// <summary>
    /// The Preview Token being used is for a non-draft DataSetVersion and the DataSet's
    /// status itself is unavailable to the public, and so the auth false.
    /// </summary>
    [Theory]
    [MemberData(nameof(DataSetStatusTheoryData.UnavailableStatuses),
        MemberType = typeof(DataSetStatusTheoryData))]
    public async Task PreviewTokenActiveButForLiveDataSetVersion_DataSetStatusUnavailable_Failure(DataSetStatus status)
    {
        DataSet dataSet = _dataFixture
            .DefaultDataSet()
            .WithStatus(status);

        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(DataSetVersionStatus.Published)
            .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken()])
            .FinishWith(dsv => dataSet.LatestLiveVersionId = dsv.Id);

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSets).ReturnsDbSet([dataSet]);
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet([dataSetVersion]);
        publicDataDbContext.SetupGet(dbContext => dbContext.PreviewTokens).ReturnsDbSet(dataSetVersion.PreviewTokens);

        var handler = BuildHandler(
            publicDataDbContext: publicDataDbContext.Object,
            requestHeaders:
            [
                PreviewTokenRequestHeader(dataSetVersion.PreviewTokens[0])
            ]);
        var context = CreateAnonymousAuthContext<ViewDataSetRequirement, DataSet>(dataSet);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static ViewDataSetAuthorizationHandler BuildHandler(
        PublicDataDbContext? publicDataDbContext = null,
        IList<KeyValuePair<string, StringValues>>? requestHeaders = null)
    {
        var dbContext = publicDataDbContext ?? Mock.Of<PublicDataDbContext>();

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        var headers = httpContextAccessor.HttpContext.Request.Headers;
        requestHeaders?.ForEach(header =>
            headers.Append(header.Key, header.Value));

        var previewTokenService = new PreviewTokenService(
            publicDataDbContext: dbContext,
            httpContextAccessor: httpContextAccessor);

        var authorizationHandlerService = new AuthorizationHandlerService(
            httpContextAccessor: httpContextAccessor,
            environment: Mock.Of<IWebHostEnvironment>(),
            previewTokenService);

        return new ViewDataSetAuthorizationHandler(authorizationHandlerService);
    }

    private static KeyValuePair<string, StringValues> PreviewTokenRequestHeader(PreviewToken previewToken)
    {
        return new KeyValuePair<string, StringValues>(RequestHeaderNames.PreviewToken, previewToken.Id.ToString());
    }
}
