using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public class ReleaseSearchableDocumentsService(
    ContentDbContext contentDbContext,
    IReleaseService releaseService
) : IReleaseSearchableDocumentsService
{
    public async Task<
        Either<ActionResult, ReleaseSearchableDocumentDto>
    > GetLatestReleaseAsSearchableDocument(
        string publicationSlug,
        CancellationToken cancellationToken = default
    ) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccessCombineWith(p =>
                releaseService.GetRelease(p.LatestPublishedReleaseVersionId!.Value)
            )
            .OnSuccess(tuple =>
                ReleaseSearchableDocumentDto.FromModel(
                    publication: tuple.Item1,
                    release: tuple.Item2
                )
            );

    private async Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken
    ) =>
        await contentDbContext
            .Publications.AsNoTracking()
            .Include(p => p.Theme)
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);
}
