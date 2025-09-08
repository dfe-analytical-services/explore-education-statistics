using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation;

public class RelatedInformationService(ContentDbContext contentDbContext) : IRelatedInformationService
{
    public async Task<Either<ActionResult, RelatedInformationDto[]>> GetRelatedInformationForRelease(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(publication =>
                GetLatestPublishedReleaseVersionByReleaseSlug(publication, releaseSlug, cancellationToken)
            )
            .OnSuccess(releaseVersion =>
                releaseVersion.RelatedInformation.Select(RelatedInformationDto.FromLink).ToArray()
            );

    private Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .Publications.AsNoTracking()
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);

    private Task<Either<ActionResult, ReleaseVersion>> GetLatestPublishedReleaseVersionByReleaseSlug(
        Publication publication,
        string releaseSlug,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .ReleaseVersions.AsNoTracking()
            .LatestReleaseVersions(publication.Id, releaseSlug, publishedOnly: true)
            .SingleOrNotFoundAsync(cancellationToken);
}
