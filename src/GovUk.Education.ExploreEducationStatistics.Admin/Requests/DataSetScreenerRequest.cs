#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record DataSetScreenerRequest
{
    public required string StorageContainerName { get; set; }

    public required string DataFileName { get; set; }

    public required string MetaFileName { get; set; }

    public required string DataFilePath { get; set; }

    public required string MetaFilePath { get; set; }

    public string DataFileSasToken { get; set; } = string.Empty;

    public string MetaFileSasToken { get; set; } = string.Empty;
}
