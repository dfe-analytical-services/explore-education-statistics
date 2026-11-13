namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

/// <summary>
/// The body of the response to a full reset of the data set documents.
/// </summary>
/// <param name="RemovedBlobCount">The number of data set document blobs which were removed.</param>
/// <param name="PublicationCount">The number of publications whose data set documents have been queued to be created again.</param>
public record FullDataSetDocumentsResetResult(int RemovedBlobCount, int PublicationCount);
