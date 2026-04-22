using System.Text;
using System.Text.Json;
using System.Web;
using GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Authentication;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Screener;

public class DataSetScreenerClient(
    HttpClient httpClient,
    IHttpClientAzureAuthenticationManager<DataScreenerOptions> authenticationManager
) : IDataSetScreenerClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // API is case-sensitive
    };

    public async Task<DataSetScreenerResponse> ScreenDataSet(
        DataSetScreenerRequest dataSetScreenerRequest,
        CancellationToken cancellationToken
    )
    {
        await authenticationManager.AddAuthentication(httpClient, cancellationToken);

        var json = JsonSerializer.Serialize(dataSetScreenerRequest, _jsonSerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // TODO (EES-6301): Add cancellation token handling logic to terminate Azure Function processes
        var response = await httpClient.PostAsync($"{httpClient.BaseAddress}/screen", content, CancellationToken.None);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<DataSetScreenerResponse>(cancellationToken)
            : throw new DataScreenerException(
                $"External data screening process failed with status code {response.StatusCode}."
            );
    }

    public async Task<List<DataSetScreenerProgressResponse>> GetScreeningProgress(
        IList<Guid> dataSetIds,
        CancellationToken cancellationToken
    )
    {
        await authenticationManager.AddAuthentication(httpClient, cancellationToken);

        var query = HttpUtility.ParseQueryString(string.Empty);
        dataSetIds.ForEach(dataSetId => query.Add("data_set_id", dataSetId.ToString()));

        var url = $"{httpClient.BaseAddress}/progress?{query}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<DataSetScreenerProgressResponse>>(cancellationToken)
            : throw new DataScreenerException(
                $"Failed to get screener progress for data set ids {dataSetIds.JoinToString(",")} - status code {response.StatusCode}."
            );
    }
}
