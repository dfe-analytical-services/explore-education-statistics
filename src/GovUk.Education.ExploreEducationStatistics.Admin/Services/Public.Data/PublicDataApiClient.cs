#nullable enable
using System.Net;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Authentication;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class PublicDataApiClient(
    ILogger<PublicDataApiClient> logger,
    HttpClient httpClient,
    IHttpClientAzureAuthenticationManager<PublicDataApiOptions> authenticationManager)
    : IPublicDataApiClient
{
    public async Task<Either<ActionResult, HttpResponseMessage>> GetDataSetVersionChanges(
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
        );
    }

    private async Task<Either<ActionResult, HttpResponseMessage>> SendRequest(
        Func<Task<HttpResponseMessage>> requestFunction,
        CancellationToken cancellationToken)
    {
        await authenticationManager.AddAuthentication(httpClient, cancellationToken);

        // Add an HTTP header to signal to the Public API that this call originates from the
        // EES service.
        httpClient.DefaultRequestHeaders.Add(RequestHeaderNames.RequestSource, "EES Admin");
            
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
