#nullable enable
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services;

public class FrontendService : IFrontendService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FrontendService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Either<ActionResult, dynamic>> CreateUniversalTableFormat(TableBuilderResultViewModel tableResult,
        TableBuilderConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("PublicApp");

        var request = new JsonNetContent(new UniversalTableFormatCreateRequest(
                new TableBuilderResultViewModel
                {
                    Results = tableResult.Results.ToList(),
                    SubjectMeta = tableResult.SubjectMeta
                },
                configuration),
            new JsonSerializerSettings
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

        // TODO EES-3753 Handle errors here?
        return new NotFoundResult();
    }

    private record UniversalTableFormatCreateRequest(
        TableBuilderResultViewModel FullTable, TableBuilderConfiguration Configuration);
}
