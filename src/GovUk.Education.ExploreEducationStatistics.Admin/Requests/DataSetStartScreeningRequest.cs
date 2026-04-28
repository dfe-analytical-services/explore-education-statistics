using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class DataSetStartScreeningRequest
{
    [JsonPropertyName("data_set_id")]
    public Guid DataSetId { get; set; }

    [JsonPropertyName("storage_container_name")]
    public required string StorageContainerName { get; set; }

    [JsonPropertyName("data_file_name")]
    public required string DataFileName { get; set; }

    [JsonPropertyName("meta_file_name")]
    public required string MetaFileName { get; set; }

    [JsonPropertyName("data_file_path")]
    public required string DataFilePath { get; set; }

    [JsonPropertyName("meta_file_path")]
    public required string MetaFilePath { get; set; }

    [JsonPropertyName("data_file_sas_token")]
    public string DataFileSasToken { get; set; } = string.Empty;

    [JsonPropertyName("meta_file_sas_token")]
    public string MetaFileSasToken { get; set; } = string.Empty;
}
