#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record DataScreenerRequest
{
    public required string StorageContainerName { get; set; }

    public required string DataFileName { get; set; }

    public required string MetaFileName { get; set; }

    public required string DataFilePath { get; set; }

    public required string MetaFilePath { get; set; }
}
