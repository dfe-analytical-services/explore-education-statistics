namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

public record RefreshDataSetDocumentsResponse(
    IReadOnlyList<string> UploadedBlobNames,
    IReadOnlyList<string> RemovedBlobNames
);
