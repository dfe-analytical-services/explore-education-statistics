#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

internal class ProcessorClient(ILogger<ProcessorClient> logger, HttpClient httpClient) : IProcessorClient
{
    public async Task<Either<ActionResult, CreateInitialDataSetVersionResponseViewModel>> CreateInitialDataSetVersion(
        Guid releaseFileId, 
        CancellationToken cancellationToken = default)
    {
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
}
