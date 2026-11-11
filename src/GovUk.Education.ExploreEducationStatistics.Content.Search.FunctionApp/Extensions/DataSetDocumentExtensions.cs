using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class DataSetDocumentExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Serialise the data set document as JSON, with property names matching the fields of the data set search index.
    /// </summary>
    public static string ToJson(this DataSetDocument dataSetDocument) =>
        JsonSerializer.Serialize(dataSetDocument, JsonSerializerOptions);
}
