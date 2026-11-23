using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

/// <summary>
/// The Azure Blob metadata keys used for data set documents.
/// </summary>
public static class DataSetDocumentAzureBlobMetadataKeys
{
    public const string FileId = "fileId";
}

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

    /// <summary>
    /// Build data set document metadata as Azure Blob metadata key/value pairs.
    /// </summary>
    /// <remarks>
    /// Metadata key/value pairs are set using HTTP headers and must be valid headers containing only ASCII characters.
    /// <para>
    /// FileId is the search index key. The native blob soft delete detection policy needs the key in blob metadata
    /// (not just the JSON content) to remove the document of a deleted blob from the index.
    /// </para>
    /// </remarks>
    public static IDictionary<string, string> BuildMetadata(this DataSetDocument dataSetDocument) =>
        new Dictionary<string, string>
        {
            { DataSetDocumentAzureBlobMetadataKeys.FileId, dataSetDocument.FileId.ToString() },
        };
}
