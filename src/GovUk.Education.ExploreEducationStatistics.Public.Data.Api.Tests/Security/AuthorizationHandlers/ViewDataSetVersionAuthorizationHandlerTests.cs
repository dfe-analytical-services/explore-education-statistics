using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Requests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Security.AuthorizationHandlers;

public class ViewDataSetVersionAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();

    [Theory]
    [MemberData(nameof(DataSetVersionStatusViewTheoryData.AvailableStatuses),
        MemberType = typeof(DataSetVersionStatusViewTheoryData))]
    public async Task Success(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var handler = BuildHandler();
        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
        MemberType = typeof(DataSetVersionStatusViewTheoryData))]
    public async Task Failure(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var handler = BuildHandler();
        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetVersionStatusViewTheoryData.AvailableStatusesIncludingDraft),
        MemberType = typeof(DataSetVersionStatusViewTheoryData))]
    public async Task Success_PreviewTokenIsActive(DataSetVersionStatus status)
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

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Failure_PreviewTokenIsExpired()
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

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Failure_PreviewTokenIsForWrongDataSetVersion()
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

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion1);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatusesExceptDraft),
        MemberType = typeof(DataSetVersionStatusViewTheoryData))]
    public async Task Failure_PreviewTokenIsForUnavailableDataSetVersion(DataSetVersionStatus status)
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

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetVersionStatusViewTheoryData.AllStatuses),
        MemberType = typeof(DataSetVersionStatusViewTheoryData))]
    public async Task Success_UserAgentHeaderInDevelopment(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var handler = BuildHandler(
            environmentName: Environments.Development,
            userAgentValue: SecurityConstants.AdminUserAgent);

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
        MemberType = typeof(DataSetVersionStatusViewTheoryData))]
    public async Task Failure_UserAgentHeaderInProduction(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var handler = BuildHandler(
            environmentName: Environments.Production,
            userAgentValue: SecurityConstants.AdminUserAgent);

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
        MemberType = typeof(DataSetVersionStatusViewTheoryData))]
    public async Task Failure_IncorrectUserAgentHeaderInDevelopment(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var handler = BuildHandler(
            environmentName: Environments.Development,
            userAgentValue: "Incorrect User Agent");

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetVersionStatusViewTheoryData.AllStatuses),
        MemberType = typeof(DataSetVersionStatusViewTheoryData))]
    public async Task Success_ClaimsPrincipalWithRole(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var userWithCorrectRole = _dataFixture.AdminAccessUser();

        var handler = BuildHandler(
            environmentName: Environments.Production,
            claimsPrincipal: userWithCorrectRole);

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
        MemberType = typeof(DataSetVersionStatusViewTheoryData))]
    public async Task Failure_ClaimsPrincipalWithIncorrectRole(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var userWithIncorrectRole = _dataFixture.UnsupportedRoleUser();

        var handler = BuildHandler(
            environmentName: Environments.Production,
            claimsPrincipal: userWithIncorrectRole);

        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static KeyValuePair<string, StringValues> PreviewTokenRequestHeader(PreviewToken previewToken)
    {
        return new KeyValuePair<string, StringValues>(RequestHeaderNames.PreviewToken, previewToken.Id.ToString());
    }

    private static ViewDataSetVersionAuthorizationHandler BuildHandler(
        IList<KeyValuePair<string, StringValues>>? requestHeaders = null,
        PublicDataDbContext? publicDataDbContext = null,
        string? environmentName = null,
        string? userAgentValue = null,
        ClaimsPrincipal? claimsPrincipal = null)
    {
        var dbContext = publicDataDbContext ?? Mock.Of<PublicDataDbContext>();

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                HttpContext =
                {
                    User = claimsPrincipal ?? new ClaimsPrincipal()
                }
            }
        };

        var headers = httpContextAccessor.HttpContext.Request.Headers;
        headers.UserAgent = userAgentValue;
        requestHeaders?.ForEach(header =>
            headers.Append(header.Key, header.Value));

        var environment = new Mock<IWebHostEnvironment>(MockBehavior.Strict);

        environment
            .SetupGet(s => s.EnvironmentName)
            .Returns(environmentName ?? Environments.Production);

        var previewTokenService = new PreviewTokenService(
            publicDataDbContext: dbContext,
            httpContextAccessor: httpContextAccessor);

        var authorizationHandlerService = new AuthorizationHandlerService(
            httpContextAccessor: httpContextAccessor,
            environment: environment.Object,
            previewTokenService);

        return new ViewDataSetVersionAuthorizationHandler(authorizationHandlerService);
    }
}
