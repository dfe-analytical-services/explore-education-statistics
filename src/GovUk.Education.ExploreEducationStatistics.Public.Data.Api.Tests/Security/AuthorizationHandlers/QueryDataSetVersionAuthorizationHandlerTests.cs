using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Constants;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Security.AuthorizationHandlers;

public class QueryDataSetVersionAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();

    [Theory]
    [MemberData(nameof(DataSetVersionStatusQueryTheoryData.AvailableStatuses),
        MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
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
    [MemberData(nameof(DataSetVersionStatusQueryTheoryData.UnavailableStatuses),
        MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
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

    [Theory]
    [MemberData(nameof(DataSetVersionStatusQueryTheoryData.AvailableStatusesIncludingDraft),
        MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
    public void Success_PreviewTokenIsActive(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        PreviewToken previewToken = _dataFixture
            .DefaultPreviewToken()
            .WithDataSetVersion(dataSetVersion);

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet([dataSetVersion]);
        publicDataDbContext.SetupGet(dbContext => dbContext.PreviewTokens).ReturnsDbSet([previewToken]);

        var handler = BuildHandler(
            publicDataDbContext: publicDataDbContext.Object,
            requestHeaders:
            [
                PreviewTokenRequestHeader(previewToken)
            ]);

        var context = CreateAnonymousAuthContext<QueryDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public void Failure_PreviewTokenIsExpired()
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(DataSetVersionStatus.Draft);

        PreviewToken previewToken = _dataFixture
            .DefaultPreviewToken(expired: true)
            .WithDataSetVersion(dataSetVersion);

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet([dataSetVersion]);
        publicDataDbContext.SetupGet(dbContext => dbContext.PreviewTokens).ReturnsDbSet([previewToken]);

        var handler = BuildHandler(
            publicDataDbContext: publicDataDbContext.Object,
            requestHeaders:
            [
                PreviewTokenRequestHeader(previewToken)
            ]);

        var context = CreateAnonymousAuthContext<QueryDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public void Failure_PreviewTokenIsForWrongDataSetVersion()
    {
        var (dataSetVersion1, dataSetVersion2) = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(DataSetVersionStatus.Draft)
            .GenerateTuple2();

        PreviewToken previewToken = _dataFixture
            .DefaultPreviewToken()
            .WithDataSetVersion(dataSetVersion2);

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSetVersions)
            .ReturnsDbSet([dataSetVersion1, dataSetVersion2]);
        publicDataDbContext.SetupGet(dbContext => dbContext.PreviewTokens).ReturnsDbSet([previewToken]);

        var handler = BuildHandler(
            publicDataDbContext: publicDataDbContext.Object,
            requestHeaders:
            [
                PreviewTokenRequestHeader(previewToken)
            ]);

        var context = CreateAnonymousAuthContext<QueryDataSetVersionRequirement, DataSetVersion>(dataSetVersion1);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetVersionStatusQueryTheoryData.UnavailableStatusesExceptDraft),
        MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
    public void Failure_PreviewTokenIsForUnavailableDataSetVersion(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        PreviewToken previewToken = _dataFixture
            .DefaultPreviewToken()
            .WithDataSetVersion(dataSetVersion);

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet([dataSetVersion]);
        publicDataDbContext.SetupGet(dbContext => dbContext.PreviewTokens).ReturnsDbSet([previewToken]);

        var handler = BuildHandler(
            publicDataDbContext: publicDataDbContext.Object,
            requestHeaders:
            [
                PreviewTokenRequestHeader(previewToken)
            ]);

        var context = CreateAnonymousAuthContext<QueryDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static KeyValuePair<string, StringValues> PreviewTokenRequestHeader(PreviewToken previewToken)
    {
        return new KeyValuePair<string, StringValues>(RequestHeaderNames.PreviewToken, previewToken.Id.ToString());
    }

    private static QueryDataSetVersionAuthorizationHandler BuildHandler(
        List<KeyValuePair<string, StringValues>>? requestHeaders = null,
        PublicDataDbContext? publicDataDbContext = null)
    {
        publicDataDbContext ??= Mock.Of<PublicDataDbContext>();

        var httpContext = new DefaultHttpContext();

        var requestFeature = new HttpRequestFeature { Headers = new HeaderDictionary(requestHeaders?.ToDictionary()) };
        httpContext.Features.Set<IHttpRequestFeature>(requestFeature);

        var previewTokenService = new PreviewTokenService(publicDataDbContext);

        return new QueryDataSetVersionAuthorizationHandler(
            new HttpContextAccessor { HttpContext = httpContext },
            previewTokenService);
    }
}
