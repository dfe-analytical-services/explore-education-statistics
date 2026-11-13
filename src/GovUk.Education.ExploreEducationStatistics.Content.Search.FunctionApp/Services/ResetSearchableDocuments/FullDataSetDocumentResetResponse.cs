using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;

public record FullDataSetDocumentResetResponse
{
    /// <summary>
    /// The publications whose data set documents need to be created again.
    /// </summary>
    public required DataSetDocumentPublication[] Publications { get; init; }

    public required int RemovedBlobCount { get; init; }
}
