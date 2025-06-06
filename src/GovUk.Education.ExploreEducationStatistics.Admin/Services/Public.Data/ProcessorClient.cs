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
    public async Task<Either<ActionResult, ProcessDataSetVersionResponseViewModel>> CreateDataSet(
        Guid releaseFileId,
        CancellationToken cancellationToken = default)
    {
        var request = new DataSetCreateRequest { ReleaseFileId = releaseFileId };

        return await SendPost<DataSetCreateRequest, ProcessDataSetVersionResponseViewModel>(
            "api/CreateDataSet",
            request,
            cancellationToken: cancellationToken);
    }

    public async Task<Either<ActionResult, ProcessDataSetVersionResponseViewModel>> CreateNextDataSetVersionMappings(
        Guid dataSetId,
        Guid releaseFileId,
        Guid? dataSetVersionToReplaceId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new NextDataSetVersionMappingsCreateRequest
        {
            ReleaseFileId = releaseFileId,
            DataSetId = dataSetId,
            DataSetVersionToReplaceId = dataSetVersionToReplaceId
        };

        return await SendPost<NextDataSetVersionMappingsCreateRequest, ProcessDataSetVersionResponseViewModel>(
            "api/CreateNextDataSetVersionMappings",
            request,
            cancellationToken: cancellationToken);
    }

    public async Task<Either<ActionResult, ProcessDataSetVersionResponseViewModel>> CompleteNextDataSetVersionImport(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var request = new NextDataSetVersionCompleteImportRequest { DataSetVersionId = dataSetVersionId };

        return await SendPost<NextDataSetVersionCompleteImportRequest, ProcessDataSetVersionResponseViewModel>(
            "api/CompleteNextDataSetVersionImport",
            request,
            cancellationToken: cancellationToken);
    }

    public async Task<Either<ActionResult, Unit>> BulkDeleteDataSetVersions(
        Guid releaseVersionId,
        bool forceDeleteAll = false,
        CancellationToken cancellationToken = default)
    {
        return await SendDelete(
            $"api/BulkDeleteDataSetVersions/{releaseVersionId}?forceDeleteAll={forceDeleteAll}",
            cancellationToken: cancellationToken);
    }

    public async Task<Either<ActionResult, Unit>> DeleteDataSetVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return await SendDelete(
            $"api/DeleteDataSetVersion/{dataSetVersionId}",
            response =>
                response.StatusCode == HttpStatusCode.NotFound ? new NotFoundResult() : null,
            cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, TResponse>> SendPost<TRequest, TResponse>(
        string url,
        TRequest request,
        Func<HttpResponseMessage, ActionResult?>? customResponseHandler = null,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        return await SendRequest<TResponse>(() =>
                httpClient.PostAsJsonAsync(
                    url,
                    request,
                    cancellationToken: cancellationToken),
            customResponseHandler,
            cancellationToken);
    }

    private async Task<Either<ActionResult, Unit>> SendDelete(
        string url,
        Func<HttpResponseMessage, ActionResult?>? customResponseHandler = null,
        CancellationToken cancellationToken = default)
    {
        return await SendRequest<Unit>(
            () => httpClient.DeleteAsync(
                url,
                cancellationToken: cancellationToken),
            customResponseHandler,
            cancellationToken);
    }

    private async Task<Either<ActionResult, TResponse>> SendRequest<TResponse>(
        Func<Task<HttpResponseMessage>> requestFunction,
        Func<HttpResponseMessage, ActionResult?>? customResponseHandler = null,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        await AddBearerToken(cancellationToken);

        var response = await requestFunction.Invoke();

        var customHandlerResponse = customResponseHandler?.Invoke(response);

        if (customHandlerResponse is not null)
        {
            return customHandlerResponse;
        }

        if (!response.IsSuccessStatusCode)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(
                        await response.Content
                            .ReadFromJsonAsync<ValidationProblemViewModel>(cancellationToken: cancellationToken)
                    );
                default:
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    var request = response.RequestMessage!;

                    logger.LogError("""
                                    Request {Method} {AbsolutePath} failed with status code {StatusCode}.

                                    Body: {Body}
                                    """,
                        request.Method,
                        request.RequestUri!.AbsolutePath,
                        response.StatusCode,
                        body);

                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        if (typeof(TResponse) == typeof(Unit))
        {
            return (Unit.Instance as TResponse)!;
        }

        var content = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);

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
