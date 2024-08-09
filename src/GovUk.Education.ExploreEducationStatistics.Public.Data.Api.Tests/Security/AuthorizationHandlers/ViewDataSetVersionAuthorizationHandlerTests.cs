using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
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
    [InlineData(DataSetVersionStatus.Deprecated)]
    [InlineData(DataSetVersionStatus.Published)]
    [InlineData(DataSetVersionStatus.Withdrawn)]
    [InlineData(DataSetVersionStatus.Processing)]
    [InlineData(DataSetVersionStatus.Failed)]
    [InlineData(DataSetVersionStatus.Mapping)]
    [InlineData(DataSetVersionStatus.Draft)]
    public void Success_UserAgentHeaderInDevelopment(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var handler = BuildHandler(
            environmentName: Environments.Development,
            userAgentValue: SecurityConstants.AdminUserAgent);
        
        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }
    
    [Theory]
    [InlineData(DataSetVersionStatus.Processing)]
    [InlineData(DataSetVersionStatus.Failed)]
    [InlineData(DataSetVersionStatus.Mapping)]
    [InlineData(DataSetVersionStatus.Draft)]
    public void Failure_UserAgentHeaderInProduction(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);
        
        var handler = BuildHandler(
            environmentName: Environments.Production,
            userAgentValue: SecurityConstants.AdminUserAgent);
        
        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
    
    [Theory]
    [InlineData(DataSetVersionStatus.Processing)]
    [InlineData(DataSetVersionStatus.Failed)]
    [InlineData(DataSetVersionStatus.Mapping)]
    [InlineData(DataSetVersionStatus.Draft)]
    public void Failure_IncorrectUserAgentHeaderInDevelopment(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);
        
        var handler = BuildHandler(
            environmentName: Environments.Development,
            userAgentValue: "Incorrect User Agent");
        
        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [InlineData(DataSetVersionStatus.Deprecated)]
    [InlineData(DataSetVersionStatus.Published)]
    [InlineData(DataSetVersionStatus.Withdrawn)]
    [InlineData(DataSetVersionStatus.Processing)]
    [InlineData(DataSetVersionStatus.Failed)]
    [InlineData(DataSetVersionStatus.Mapping)]
    [InlineData(DataSetVersionStatus.Draft)]
    public void Success_ClaimsPrincipalWithRole(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var userWithCorrectRole = new ClaimsPrincipal(
            identity: new ClaimsIdentity(
                authenticationType: JwtBearerDefaults.AuthenticationScheme,
                claims: new[] { new Claim(ClaimTypes.Role, SecurityConstants.UnpublishedDataReaderAppRole) },
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role));
        
        var handler = BuildHandler(
            environmentName: Environments.Production,
            claimsPrincipal: userWithCorrectRole);
        
        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }
    
    [Theory]
    [InlineData(DataSetVersionStatus.Processing)]
    [InlineData(DataSetVersionStatus.Failed)]
    [InlineData(DataSetVersionStatus.Mapping)]
    [InlineData(DataSetVersionStatus.Draft)]
    public void Failure_ClaimsPrincipalWithIncorrectRole(DataSetVersionStatus status)
    {
        DataSetVersion dataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithStatus(status);

        var userWithIncorrectRole = new ClaimsPrincipal(
            identity: new ClaimsIdentity(
                authenticationType: JwtBearerDefaults.AuthenticationScheme,
                claims: new[] { new Claim(ClaimTypes.Role, "Incorrect Role") },
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role));
        
        var handler = BuildHandler(
            environmentName: Environments.Production,
            claimsPrincipal: userWithIncorrectRole);
        
        var context = CreateAnonymousAuthContext<ViewDataSetVersionRequirement, DataSetVersion>(dataSetVersion);

        handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static ViewDataSetVersionAuthorizationHandler BuildHandler(
        string? environmentName = null,
        string? userAgentValue = null,
        ClaimsPrincipal? claimsPrincipal = null)
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                HttpContext =
                {
                    User = claimsPrincipal!,
                    Request =
                    {
                        Headers =
                        {
                            UserAgent = userAgentValue
                        }
                    }
                }
            }
        };

        var environment = new Mock<IWebHostEnvironment>(MockBehavior.Strict);
        
        environment
            .SetupGet(s => s.EnvironmentName)
            .Returns(environmentName ?? Environments.Production);

        var authorizationService = new AuthorizationService(
            httpContextAccessor: httpContextAccessor, 
            environment: environment.Object);

        return new ViewDataSetVersionAuthorizationHandler(authorizationService);
    }
}
