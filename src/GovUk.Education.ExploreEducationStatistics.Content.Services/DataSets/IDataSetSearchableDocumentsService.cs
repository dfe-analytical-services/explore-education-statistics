using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets;

public interface IDataSetSearchableDocumentsService
{
    Task<Either<ActionResult, DataSetSearchableDocumentDto[]>> GetReleaseDataSetSearchableDocuments(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetSearchableDocumentDto[]>> GetPublicationDataSetSearchableDocuments(
        string publicationSlug,
        CancellationToken cancellationToken = default
    );

    Task<DataSetSearchableDocumentKeyDto[]> GetDataSetSearchableDocumentKeys(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lists the publications which have data set searchable documents.
    /// </summary>
    Task<DataSetSearchableDocumentPublicationDto[]> GetDataSetSearchableDocumentPublications(
        CancellationToken cancellationToken = default
    );
}
