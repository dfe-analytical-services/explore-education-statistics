using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Authentication;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetScreenerClient(
    HttpClient httpClient,
    IHttpClientAzureAuthenticationManager<DataScreenerClientOptions> authenticationManager
) : IDataSetScreenerClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // API is case sensitive
    };

    public async Task<DataSetScreenerResponse> ScreenDataSet(
        DataSetScreenerRequest dataSetRequest,
        CancellationToken cancellationToken
    )
    {
        // TODO (EES-6341): Re-enable screening once fully implemented.
        return new DataSetScreenerResponse { OverallResult = "Passed", Passed = true };
        //await authenticationManager.AddAuthentication(httpClient, cancellationToken);

        //var json = JsonSerializer.Serialize(dataSetRequest, _jsonSerializerOptions);
        //var content = new StringContent(json, Encoding.UTF8, "application/json");

        //// TODO (EES-5353): Add cancellation token handling logic to terminate Azure Function processes
        //var response = await httpClient.PostAsync(httpClient.BaseAddress, content, CancellationToken.None);

        //return response.IsSuccessStatusCode
        //    ? await response.Content.ReadFromJsonAsync<DataSetScreenerResponse>(cancellationToken)
        //    : throw new Exception($"Screening process failed with status {response.StatusCode}");
    }
}
