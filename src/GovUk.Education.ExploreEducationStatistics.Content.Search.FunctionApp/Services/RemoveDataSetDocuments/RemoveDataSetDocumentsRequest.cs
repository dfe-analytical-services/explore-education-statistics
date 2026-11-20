using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;

public record RemoveDataSetDocumentsRequest
{
    /// <summary>
    /// The publication whose data set documents can be removed. This is always required.
    /// </summary>
    public required Guid PublicationId { get; init; }

    /// <summary>
    /// If specified, only the data set documents of this release are removed.
    /// </summary>
    public Guid? ReleaseId { get; init; }

    /// <summary>
    /// If specified, only the data set document for this data file is removed.
    /// PublicationId and ReleaseId are always required when FileId is specified.
    /// </summary>
    public Guid? FileId { get; init; }

    /// <summary>
    /// The data set documents to keep when removing those of a publication or release.
    /// </summary>
    public IReadOnlyCollection<DataSetDocumentKey> Except { get; init; } = [];
}
