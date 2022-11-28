#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class PublicationRepository : IPublicationRepository
{
    private readonly ContentDbContext _contentDbContext;

    public PublicationRepository(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<bool> IsPublished(Guid publicationId)
    {
        return await _contentDbContext.Publications
            .AnyAsync(p => p.Id == publicationId && p.LatestPublishedReleaseId != null);
    }

    public async Task<bool> IsSuperseded(Guid publicationId)
    {
        // To be superseded, a superseding publication must exist and have a published release
        return await _contentDbContext
            .Publications
            .Include(publication => publication.SupersededBy)
            .AnyAsync(publication => publication.Id == publicationId &&
                                     publication.SupersededBy != null &&
                                     publication.SupersededBy.LatestPublishedReleaseId != null);
    }

    public async Task UpdateLatestPublishedRelease(Guid publicationId)
    {
        var publication = await _contentDbContext.Publications
            .Include(p => p.Releases)
            .SingleAsync(p => p.Id == publicationId);

        var publishedReleases = publication.GetPublishedReleases();

        if (!publishedReleases.Any())
        {
            throw new InvalidOperationException(
                $"Expected publication to have at least one published release. Publication id: {publicationId}");
        }

        var latestPublishedRelease = publishedReleases
            .OrderBy(r => r.Year)
            .ThenBy(r => r.TimePeriodCoverage)
            .Last();

        publication.LatestPublishedReleaseId = latestPublishedRelease.Id;
        _contentDbContext.Update(publication);
        await _contentDbContext.SaveChangesAsync();
    }
}
