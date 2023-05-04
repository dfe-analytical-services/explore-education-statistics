#nullable enable
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services;

public class FrontendService : IFrontendService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FrontendService> _logger;

    public FrontendService(IHttpClientFactory httpClientFactory,
        ILogger<FrontendService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Task<Either<ActionResult, dynamic>> CreateUniversalTable(LegacyPermalink legacyPermalink,
        CancellationToken cancellationToken = default)
    {
        return CreateUniversalTable(legacyPermalink.FullTable.AsTableBuilderResultViewModel(),
            legacyPermalink.Configuration,
            cancellationToken);
    }

    public async Task<Either<ActionResult, dynamic>> CreateUniversalTable(TableBuilderResultViewModel tableResult,
        TableBuilderConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient("PublicApp");

        var request = new JsonNetContent(
            content: new UniversalTableFormatCreateRequest(tableResult, configuration),
            settings: new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });

        var response = await httpClient.PostAsync("api/permalink",
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var tableJson = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<dynamic>(tableJson)!;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new NotFoundResult();
        }

        _logger.LogError(
            "Frontend responded with error status code {StatusCode}. Body: {Body}",
            (int) response.StatusCode,
            await response.Content.ReadAsStringAsync(cancellationToken));

        // To avoid potentially leaking the frontend endpoint's implementation details to the client,
        // return any non 200/404 responses as a 500.
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }

    private record UniversalTableFormatCreateRequest(
        TableBuilderResultViewModel FullTable, TableBuilderConfiguration Configuration);
}
