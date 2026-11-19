using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

/// <param name="Keys">The keys of the data set documents that were uploaded.</param>
public record CreateDataSetDocumentsResponse(IReadOnlyList<DataSetDocumentKey> Keys);
