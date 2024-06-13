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
    public async Task<Either<ActionResult, CreateDataSetResponseViewModel>> CreateDataSet(
        Guid releaseFileId,
        CancellationToken cancellationToken = default)
    {
        var request = new DataSetCreateRequest {ReleaseFileId = releaseFileId};

        return await HandlePost<DataSetCreateRequest, CreateDataSetResponseViewModel>(
            "api/CreateDataSet",
            "Creating data set",
            request,
            cancellationToken);
    }

    public async Task<Either<ActionResult, CreateDataSetResponseViewModel>> CreateNextDataSetVersion(Guid dataSetId,
        Guid releaseFileId,
        CancellationToken cancellationToken = default)
    {
        var request = new NextDataSetVersionCreateRequest {ReleaseFileId = releaseFileId, DataSetId = dataSetId};

        return await HandlePost<NextDataSetVersionCreateRequest, CreateDataSetResponseViewModel>(
            "api/CreateNextDataSetVersion",
            "Creating next data set version",
            request,
            cancellationToken);
    }

    public async Task<Either<ActionResult, Unit>> DeleteDataSetVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return await HandleDelete(
            $"api/DeleteDataSetVersion/{dataSetVersionId}",
            "Deleting data set version",
            cancellationToken);
    }

    private async Task<Either<ActionResult, TResponse>> HandlePost<TRequest, TResponse>(
        string url,
        string actionDescription,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        return await HandleRequest<TResponse>(() =>
                httpClient.PostAsJsonAsync(
                    url,
                    request,
                    cancellationToken: cancellationToken),
            actionDescription,
            cancellationToken);
    }

    private async Task<Either<ActionResult, Unit>> HandleDelete(
        string url,
        string actionDescription,
        CancellationToken cancellationToken = default)
    {
        return await HandleRequest<Unit>(
                () => httpClient.DeleteAsync(
                    url,
                    cancellationToken: cancellationToken),
                actionDescription,
                cancellationToken)
            .OnSuccessVoid();
    }

    private async Task<Either<ActionResult, TResponse>> HandleRequest<TResponse>(
        Func<Task<HttpResponseMessage>> requestFunction,
        string actionDescription,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        await AddBearerToken(cancellationToken);

        var response = await requestFunction.Invoke();

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
                                     Failed while performing action "{actionDescription}" with status code:
                                     {response.StatusCode}. Message: {message}
                                     """);
                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        if (typeof(TResponse) == typeof(Unit))
        {
            return (Unit.Instance as TResponse)!;
        }

        var content = await response.Content.ReadFromJsonAsync<TResponse>(
            cancellationToken: cancellationToken
        );

        return content
               ?? throw new Exception("Could not deserialize the response from the Public Data Processor.");
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
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.Token);
        }
    }
}
