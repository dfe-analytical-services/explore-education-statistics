using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public class ReleaseSearchableDocumentsService(ContentDbContext contentDbContext) : IReleaseSearchableDocumentsService
{
    public async Task<Either<ActionResult, ReleaseSearchableDocumentDto>> GetLatestReleaseAsSearchableDocument(
        string publicationSlug,
        CancellationToken cancellationToken = default
    ) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(publication =>
                GetReleaseVersionWithContent(publication.LatestPublishedReleaseVersionId!.Value, cancellationToken)
                    .OnSuccess(ReleaseSearchableDocumentDto.FromReleaseVersion)
            );

    private async Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken
    ) =>
        await contentDbContext
            .Publications.AsNoTracking()
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);

    private Task<Either<ActionResult, ReleaseVersion>> GetReleaseVersionWithContent(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .ReleaseVersions.AsNoTracking()
            .Include(rv => rv.Release.Publication.Theme)
            .Include(rv => rv.Content)
                .ThenInclude(cs => cs.Content)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken);
}
