using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
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

    [Theory]
    [InlineData(DataSetVersionStatus.Draft)]
    [InlineData(DataSetVersionStatus.Deprecated)]
    [InlineData(DataSetVersionStatus.Published)]
    [InlineData(DataSetVersionStatus.Withdrawn)]
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

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

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
            .DefaultPreviewToken()
            .WithDataSetVersion(dataSetVersion)
            .WithExpiry(DateTimeOffset.UtcNow.AddSeconds(-1));

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet([dataSetVersion]);
        publicDataDbContext.SetupGet(dbContext => dbContext.PreviewTokens).ReturnsDbSet([previewToken]);

        var handler = BuildHandler(
            publicDataDbContext: publicDataDbContext.Object,
            requestHeaders:
            [
                PreviewTokenRequestHeader(previewToken)
            ]);

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public void Failure_PreviewTokenIsForWrongDataSetVersion()
    {
        var (dataSetVersion1, dataSetVersion2) = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(DataSetVersionStatus.Draft)
            .Generate(2)
            .ToTuple2();

        PreviewToken previewToken = _dataFixture
            .DefaultPreviewToken()
            .WithDataSetVersion(dataSetVersion2)
            .WithExpiry(DateTimeOffset.UtcNow.AddSeconds(-1));

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

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion1);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [InlineData(DataSetVersionStatus.Processing)]
    [InlineData(DataSetVersionStatus.Failed)]
    [InlineData(DataSetVersionStatus.Mapping)]
    public void Failure_PreviewTokenIsForInaccessibleDataSetVersion(DataSetVersionStatus status)
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

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static KeyValuePair<string, StringValues> PreviewTokenRequestHeader(PreviewToken previewToken)
    {
        return new KeyValuePair<string, StringValues>(
            RequestHeaderConstants.PreviewTokenRequestHeaderName,
            previewToken.Id.ToString());
    }

    private static ViewDataSetVersionAuthorizationHandler BuildHandler(
        IList<KeyValuePair<string, StringValues>>? requestHeaders = null,
        PublicDataDbContext? publicDataDbContext = null)
    {
        publicDataDbContext ??= Mock.Of<PublicDataDbContext>();

        var httpContext = new DefaultHttpContext();

        var requestFeature = new HttpRequestFeature { Headers = new HeaderDictionary(requestHeaders?.ToDictionary()) };
        httpContext.Features.Set<IHttpRequestFeature>(requestFeature);

        var previewTokenService = new PreviewTokenService(publicDataDbContext);

        return new ViewDataSetVersionAuthorizationHandler(
            new HttpContextAccessor { HttpContext = httpContext },
            previewTokenService);
    }
}
