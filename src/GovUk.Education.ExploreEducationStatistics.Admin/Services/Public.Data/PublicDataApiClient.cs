#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class PublicDataApiClient(
    ILogger<PublicDataApiClient> logger,
    HttpClient httpClient)
    : IPublicDataApiClient
{
    public async Task<Either<ActionResult, HttpResponseMessage>> GetDataSetVersionChanges(
        Guid dataSetId,
        string dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        return await SendRequest(
            () => httpClient.GetAsync(
                $"api/v1/data-sets/{dataSetId}/versions/{dataSetVersion}/changes",
                cancellationToken
            ),
            cancellationToken
        );
    }

    private async Task<Either<ActionResult, HttpResponseMessage>> SendRequest(
        Func<Task<HttpResponseMessage>> requestFunction,
        CancellationToken cancellationToken)
    {
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
}
