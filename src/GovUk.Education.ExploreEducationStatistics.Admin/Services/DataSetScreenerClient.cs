using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetScreenerClient(HttpClient client) : IDataSetScreenerClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // API is case sensitive
    };

    public async Task<DataSetScreenerResponse> ScreenDataSet(
        DataSetScreenerRequest dataSetRequest,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(dataSetRequest, _jsonSerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // TODO (EES-5353): Add cancellation token handling logic to terminate Azure Function processes
        var response = await client.PostAsync(client.BaseAddress, content, CancellationToken.None);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<DataSetScreenerResponse>(cancellationToken)
            : throw new Exception($"Screening process failed with status {response.StatusCode}");
    }
}
