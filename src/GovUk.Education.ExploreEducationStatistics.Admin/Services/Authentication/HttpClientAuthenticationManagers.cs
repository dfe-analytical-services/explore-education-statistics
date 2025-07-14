using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Security;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Authentication;

// ReSharper disable once UnusedTypeParameter
public interface IHttpClientAzureAuthenticationManager<TOptions>
    where TOptions : class, IAzureAuthenticationOptions
{
    Task AddAuthentication(HttpClient httpClient, CancellationToken cancellationToken);
}

public class HttpClientDefaultAzureCredentialAuthenticationManager<TOptions>(
    IOptions<TOptions> options) : IHttpClientAzureAuthenticationManager<TOptions>
    where TOptions : class, IAzureAuthenticationOptions
{
    /// <summary>
    /// Request an access token to authenticate the Admin App Service with the Data Processor using its
    /// system-assigned identity.
    ///
    /// We call this within this class rather than during "AddHttpClient" initialisation
    /// within <see cref="Startup" /> because adding the call for the Bearer token in there instead would otherwise
    /// result in a Bearer token unnecessarily being requested every time ProcessorClient was instantiated but not
    /// used (e.g. by virtue of a Controller using DataSetService as a dependency but not calling any service methods
    /// that require interaction with ProcessorClient).
    ///
    /// The other advantage of requesting the Bearer token here rather than in "AddHttpClient" is that it can be done
    /// asynchronously here.
    /// 
    /// </summary>
    public async Task AddAuthentication(
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        var accessTokenProvider = new DefaultAzureCredential();
        var tokenResponse = await accessTokenProvider.GetTokenAsync(
            new TokenRequestContext([$"api://{options.Value.AppRegistrationClientId}/.default"]),
            cancellationToken);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.Token);
    }
}

public class HttpHeaderHttpClientAuthenticationManager<TOptions> : IHttpClientAzureAuthenticationManager<TOptions>
    where TOptions : class, IAzureAuthenticationOptions
{
    public Task AddAuthentication(HttpClient httpClient, CancellationToken cancellationToken)
    {
        httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, SecurityConstants.AdminUserAgent);
        return Task.CompletedTask;
    }
}
