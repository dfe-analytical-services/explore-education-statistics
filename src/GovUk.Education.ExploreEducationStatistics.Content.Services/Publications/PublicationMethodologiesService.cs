using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public class PublicationMethodologiesService(ContentDbContext contentDbContext)
    : IPublicationMethodologiesService
{
    public async Task<
        Either<ActionResult, PublicationMethodologiesDto>
    > GetPublicationMethodologies(
        string publicationSlug,
        CancellationToken cancellationToken = default
    ) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(async publication =>
            {
                var methodologies = await GetPublishedMethodologies(publication, cancellationToken);
                return new PublicationMethodologiesDto
                {
                    Methodologies = methodologies
                        .OrderBy(m => m.LatestPublishedVersion!.Title)
                        .Select(PublicationMethodologyDto.FromMethodology)
                        .ToArray(),
                    ExternalMethodology =
                        publication.ExternalMethodology != null
                            ? PublicationExternalMethodologyDto.FromExternalMethodology(
                                publication.ExternalMethodology
                            )
                            : null,
                };
            });

    private Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .Publications.AsNoTracking()
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);

    private Task<Methodology[]> GetPublishedMethodologies(
        Publication publication,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .Entry(publication)
            .Collection(p => p.Methodologies)
            .Query()
            .AsNoTracking()
            .Include(pm => pm.Methodology)
            .ThenInclude(m => m.LatestPublishedVersion)
            .ThenInclude(mv => mv!.Methodology)
            .Select(pm => pm.Methodology)
            .WhereHasPublishedMethodologyVersion()
            .ToArrayAsync(cancellationToken);
}
