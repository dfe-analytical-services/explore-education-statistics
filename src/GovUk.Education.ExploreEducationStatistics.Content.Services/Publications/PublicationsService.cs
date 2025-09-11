using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public class PublicationsService(ContentDbContext contentDbContext) : IPublicationsService
{
    public async Task<Either<ActionResult, PublicationDto>> GetPublication(
        string publicationSlug,
        CancellationToken cancellationToken = default)
    {
        return await GetPublicationBySlug(publicationSlug,
                includeContact: true,
                includeLatestPublishedRelease: true,
                includeTheme: true,
                cancellationToken)
            .OnSuccess(async publication =>
            {
                var supersededByPublication = await GetSupersededByPublication(publication.Id, cancellationToken);
                return PublicationDto.FromPublication(
                    publication: publication,
                    supersededByPublication: supersededByPublication);
            });
    }

    private async Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        bool includeContact = false,
        bool includeLatestPublishedRelease = false,
        bool includeTheme = false,
        CancellationToken cancellationToken = default)
    {
        var query = contentDbContext.Publications
            .AsNoTracking()
            .Where(p => p.Slug == publicationSlug && p.LatestPublishedReleaseVersionId.HasValue);

        if (includeContact)
        {
            query = query.Include(p => p.Contact);
        }

        if (includeLatestPublishedRelease)
        {
            query = query.Include(p => p.LatestPublishedReleaseVersion!.Release);
        }

        if (includeTheme)
        {
            query = query.Include(p => p.Theme);
        }

        return await query.SingleOrNotFoundAsync(cancellationToken);
    }

    private async Task<Publication?> GetSupersededByPublication(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return await contentDbContext.Publications
            .AsNoTracking()
            .Where(p => p.Id == publicationId &&
                        p.SupersededBy != null &&
                        p.SupersededBy.LatestPublishedReleaseVersionId.HasValue)
            .Select(p => p.SupersededBy)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
