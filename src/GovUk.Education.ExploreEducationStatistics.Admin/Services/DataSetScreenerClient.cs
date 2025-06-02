using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetScreenerClient(IHttpClientFactory httpClientFactory) : IDataSetScreenerClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // API is case sensitive
    };

    public async Task<List<DataSetUploadResultViewModel>> ScreenDataSet(
        List<DataSetUploadResultViewModel> dataSets)
    {
        var dataSet = dataSets[0];

        var request = new DataSetScreenerRequest
        {
            DataFileName = dataSet.DataFileName,
            DataFilePath = dataSet.DataFilePath,
            MetaFileName = dataSet.MetaFileName,
            MetaFilePath = dataSet.MetaFilePath,
        };

        var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var client = httpClientFactory.CreateClient("DataSetScreener");
        var response = await client.PostAsync("http://localhost/api/screen", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<DataSetScreenerResult>();
            dataSet.ScreenerResult = result;
            return dataSets;
        }

        throw new Exception("Screening process failed");
    }
}
