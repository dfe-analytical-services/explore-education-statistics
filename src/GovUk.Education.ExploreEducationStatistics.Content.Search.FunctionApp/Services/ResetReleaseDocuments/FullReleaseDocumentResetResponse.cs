using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetReleaseDocuments;

public record FullReleaseDocumentResetResponse
{
    /// <summary>
    /// The publications whose release documents need to be created again.
    /// </summary>
    public required PublicationInfo[] Publications { get; init; }

    public required int RemovedBlobCount { get; init; }
}
