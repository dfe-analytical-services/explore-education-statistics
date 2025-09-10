using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public interface IReleaseSearchableDocumentsService
{
    Task<Either<ActionResult, ReleaseSearchableDocumentDto>> GetLatestReleaseAsSearchableDocument(
        string publicationSlug,
        CancellationToken cancellationToken = default);
}
