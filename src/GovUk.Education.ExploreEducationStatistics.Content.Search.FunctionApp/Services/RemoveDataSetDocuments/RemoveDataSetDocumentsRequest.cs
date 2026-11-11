using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;

public record RemoveDataSetDocumentsRequest
{
    public required Guid PublicationId { get; init; }

    /// <summary>
    /// If specified, only the data set documents of this release are removed.
    /// </summary>
    public Guid? ReleaseId { get; init; }

    /// <summary>
    /// If specified, only the data set document of this data file is removed. A release id is then required.
    /// </summary>
    public Guid? FileId { get; init; }

    /// <summary>
    /// The data set documents to keep when removing those of a publication or release.
    /// </summary>
    public IReadOnlyCollection<DataSetDocumentKey> Except { get; init; } = [];
}
