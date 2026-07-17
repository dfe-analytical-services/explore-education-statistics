#nullable enable
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

    public async Task<List<DataSetScreenerProgressResponse>> GetScreenerProgress(
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
            ? (await response.Content.ReadFromJsonAsync<List<DataSetScreenerProgressResponse>>(cancellationToken))!
            : throw new DataScreenerException(
                $"Failed to get screener progress for data set ids {dataSetIds.JoinToString(",")} - status code {response.StatusCode}."
            );
    }

    public async Task<List<DataSetScreenerCompletionReportResponse>> GetScreenerCompletionReports(
        IList<Guid> dataSetIds,
        CancellationToken cancellationToken
    )
    {
        await authenticationManager.AddAuthentication(httpClient, cancellationToken);

        var query = HttpUtility.ParseQueryString(string.Empty);
        dataSetIds.ForEach(dataSetId => query.Add("data_set_id", dataSetId.ToString()));

        var url = $"{httpClient.BaseAddress}/completion-reports?{query}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        return response.IsSuccessStatusCode
            ? (
                await response.Content.ReadFromJsonAsync<List<DataSetScreenerCompletionReportResponse>>(
                    cancellationToken
                )
            )!
            : throw new DataScreenerException(
                $"Failed to get screener completion reports for data set ids {dataSetIds.JoinToString(",")} - status code {response.StatusCode}."
            );
    }

    public async Task DeleteScreenerProgressAndCompletionFiles(
        IList<Guid> dataSetIds,
        CancellationToken cancellationToken
    )
    {
        await authenticationManager.AddAuthentication(httpClient, cancellationToken);

        var query = HttpUtility.ParseQueryString(string.Empty);
        dataSetIds.ForEach(dataSetId => query.Add("data_set_id", dataSetId.ToString()));

        var url = $"{httpClient.BaseAddress}/progress-and-completion-files?{query}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);

        // Add a dummy empty body to prevent Azure from rewriting the empty DELETE request into a
        // request with the "Transfer-Encoding: chunked" header set, which the Azure Functions Host
        // can't handle.
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new DataScreenerException(
                $"Failed to delete screener progress and completion files for data set ids {dataSetIds.JoinToString(",")} - status code {response.StatusCode}."
            );
        }
    }
}
