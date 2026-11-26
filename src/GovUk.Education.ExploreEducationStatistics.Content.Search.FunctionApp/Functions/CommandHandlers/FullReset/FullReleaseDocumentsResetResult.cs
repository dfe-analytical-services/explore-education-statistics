namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

/// <summary>
/// The body of the response to a full reset of the release documents.
/// </summary>
/// <param name="RemovedBlobCount">The number of release document blobs which were removed.</param>
/// <param name="PublicationCount">The number of publications whose release documents have been queued to be created again.</param>
public record FullReleaseDocumentsResetResult(int RemovedBlobCount, int PublicationCount);
