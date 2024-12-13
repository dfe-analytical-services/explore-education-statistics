#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class ReleaseRepository(ContentDbContext context) : IReleaseRepository
{
    public Task<List<Release>> ListPublishedReleases(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return QueryPublishedReleases(publicationId)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Guid>> ListPublishedReleaseIds(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return QueryPublishedReleases(publicationId)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Release> QueryPublishedReleases(Guid publicationId)
    {
        // For simplicity, we only query releases that have ANY version that has been published.
        // In future this may need to change if release versions can be recalled/unpublished.
        return context.Releases
            .Where(r => r.PublicationId == publicationId)
            .Where(r => r.Versions.Any(rv => rv.Published != null));
    }
}
