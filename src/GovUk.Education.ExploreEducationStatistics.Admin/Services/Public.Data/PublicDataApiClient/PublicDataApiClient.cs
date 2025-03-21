#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data.PublicDataApiClient;

public class PublicDataApiClient(
    ILogger<PublicDataApiClient> logger,
    HttpClient httpClient,
    IOptions<PublicDataApiOptions> options,
    IWebHostEnvironment environment)
    : IPublicDataApiClient
{
    public async Task<Either<ActionResult, DataSetVersionChangesViewModelDto?>> GetDataSetVersionChanges(
        Guid dataSetId,
        string dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        return await SendRequest(
            () => httpClient.GetAsync(
                $"v1/data-sets/{dataSetId}/versions/{dataSetVersion}/changes",
                cancellationToken
            ),
            cancellationToken
        ).OnSuccess(async response => await response.Content.ReadFromJsonAsync<DataSetVersionChangesViewModelDto>());
    }

    private async Task<Either<ActionResult, HttpResponseMessage>> SendRequest(
        Func<Task<HttpResponseMessage>> requestFunction,
        CancellationToken cancellationToken)
    {
        await AddBearerToken(cancellationToken);

        var response = await requestFunction();

        if (!response.IsSuccessStatusCode)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(
                        await response.Content.ReadFromJsonAsync<ValidationProblemViewModel>(cancellationToken)
                    );
                default:
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    var request = response.RequestMessage!;

                    logger.LogError(
                        """
                        Request {Method} {AbsolutePath} failed with status code {StatusCode}.

                        Body: {Body}
                        """,
                        request.Method,
                        request.RequestUri!.AbsolutePath,
                        response.StatusCode,
                        body
                    );

                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        return response;
    }

    /// <summary>
    /// Request an access token to authenticate the Admin App Service with the Public API using its
    /// system-assigned identity.
    /// </summary>
    private async Task AddBearerToken(CancellationToken cancellationToken)
    {
        // If operating within Azure, obtain an access token for authenticating the Admin App Service with
        // the Public API using its managed identity.
        //
        // By virtue of the Admin App's managed identity being granted the "Admin.Access" App Role
        // on the Public API, the access token retrieved here will have the App Role present in its list
        // of Role claims.
        if (environment.IsProduction())
        {
            var accessTokenProvider = new DefaultAzureCredential();
            var tokenResponse = await accessTokenProvider.GetTokenAsync(
                new TokenRequestContext([$"api://{options.Value.AppRegistrationClientId}/.default"]),
                cancellationToken);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.Token);
        }
    }
}
