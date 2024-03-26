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

public class CacheKeyService : ICacheKeyService
{
    private readonly ContentDbContext _contentDbContext;

    public CacheKeyService(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<Either<ActionResult, ReleaseSubjectsCacheKey>> CreateCacheKeyForReleaseSubjects(
        Guid releaseVersionId)
    {
        var releaseVersion = await _contentDbContext
            .ReleaseVersions
            .Include(rv => rv.Publication)
            .SingleAsync(rv => rv.Id == releaseVersionId);

        return new ReleaseSubjectsCacheKey(releaseVersion.Publication.Slug, releaseVersion.Slug, releaseVersion.Id);
    }
}
