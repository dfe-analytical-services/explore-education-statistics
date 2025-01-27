#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services;

public class CacheKeyService(ContentDbContext contentDbContext) : ICacheKeyService
{
    public async Task<Either<ActionResult, ReleaseSubjectsCacheKey>> CreateCacheKeyForReleaseSubjects(
        Guid releaseVersionId)
    {
        var releaseVersion = await contentDbContext
            .ReleaseVersions
            .Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .SingleAsync(rv => rv.Id == releaseVersionId);

        return new ReleaseSubjectsCacheKey(
            publicationSlug: releaseVersion.Release.Publication.Slug,
            releaseSlug: releaseVersion.Release.Slug,
            releaseVersionId: releaseVersion.Id);
    }
}
