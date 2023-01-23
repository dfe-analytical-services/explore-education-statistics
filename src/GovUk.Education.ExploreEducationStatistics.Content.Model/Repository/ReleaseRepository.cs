#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class ReleaseRepository : IReleaseRepository
{
    private readonly ContentDbContext _contentDbContext;

    public ReleaseRepository(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<bool> IsLatestPublishedVersionOfRelease(Guid releaseId)
    {
        var release = await _contentDbContext.Releases
            .SingleAsync(r => r.Id == releaseId);

        var publication = await _contentDbContext.Publications
            .Include(p => p.Releases)
            .SingleAsync(p => p.Id == release.PublicationId);

        return publication.IsLatestPublishedVersionOfRelease(release);
    }

    public async Task<Either<ActionResult, Release>> GetLatestPublishedRelease(Guid publicationId)
    {
        var publication = await _contentDbContext.Publications
            .Include(p => p.LatestPublishedRelease)
            .SingleOrDefaultAsync(p => p.Id == publicationId);

        return publication?.LatestPublishedRelease ?? new Either<ActionResult, Release>(new NotFoundResult());
    }
}
