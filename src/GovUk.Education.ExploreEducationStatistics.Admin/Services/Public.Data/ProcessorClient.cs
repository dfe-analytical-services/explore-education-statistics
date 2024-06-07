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
using GovUk.Education.ExploreEducationStatistics.Admin.Settings;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

internal class ProcessorClient(
    ILogger<ProcessorClient> logger,
    HttpClient httpClient, 
    IOptions<PublicDataProcessorOptions> options,
    IWebHostEnvironment environment) : IProcessorClient
{
    public async Task<Either<ActionResult, CreateInitialDataSetVersionResponseViewModel>> CreateInitialDataSetVersion(
        Guid releaseFileId, 
        CancellationToken cancellationToken = default)
    {
        await AddBearerToken(cancellationToken);

        var request = new InitialDataSetVersionCreateRequest
        {
            ReleaseFileId = releaseFileId,
        };

        var response = await httpClient
            .PostAsJsonAsync("api/CreateInitialDataSetVersion", request, cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(
                        await response.Content
                            .ReadFromJsonAsync<ValidationProblemViewModel>(cancellationToken: cancellationToken)
                    );
                case HttpStatusCode.NotFound:
                    return new NotFoundResult();
                default:
                    var message = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogError($"""
                         Failed to create data set version with status code: {response.StatusCode}. Message:
                         {message}
                         """);
                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        var content = await response.Content.ReadFromJsonAsync<CreateInitialDataSetVersionResponseViewModel>(
                cancellationToken: cancellationToken
            );

        return content
            ?? throw new Exception("Could not deserialize the response from the Public Data Processor.");
    }

    public async Task<Either<ActionResult, Unit>> DeleteDataSetVersion(
        Guid dataSetVersionId, 
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient
            .DeleteAsync($"api/DeleteDataSetVersion/{dataSetVersionId}", cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(
                        await response.Content
                            .ReadFromJsonAsync<ValidationProblemViewModel>(cancellationToken: cancellationToken)
                    );
                case HttpStatusCode.NotFound:
                    return new NotFoundResult();
                default:
                    var message = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogError($"""
                         Failed to delete data set version with status code: {response.StatusCode}. Message:
                         {message}
                         """);
                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        return Unit.Instance;
    }

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
    private async Task AddBearerToken(CancellationToken cancellationToken)
    {
        // If operating within Azure, obtain an access token for authenticating the Admin App Service with
        // the Public Data Processor using its managed identity.
        if (environment.IsProduction())
        {
            var accessTokenProvider = new DefaultAzureCredential();
            var tokenResponse = await accessTokenProvider.GetTokenAsync(
                new TokenRequestContext([$"api://{options.Value.AppRegistrationClientId}/.default"]), 
                cancellationToken);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);
        }
    }
}
