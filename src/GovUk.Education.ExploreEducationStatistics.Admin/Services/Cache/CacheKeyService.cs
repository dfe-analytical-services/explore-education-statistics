#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Cache;

public class CacheKeyService : ICacheKeyService
{
    private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;

    public CacheKeyService(IPersistenceHelper<ContentDbContext> contentPersistenceHelper)
    {
        _contentPersistenceHelper = contentPersistenceHelper;
    }

    public async Task<
        Either<ActionResult, DataBlockTableResultCacheKey>
    > CreateCacheKeyForDataBlock(Guid releaseVersionId, Guid dataBlockVersionId)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<DataBlockVersion>(query =>
                query.Where(dataBlockVersion =>
                    dataBlockVersion.Id == dataBlockVersionId
                    && dataBlockVersion.ReleaseVersionId == releaseVersionId
                )
            )
            .OnSuccess(dataBlockVersion => new DataBlockTableResultCacheKey(dataBlockVersion));
    }
}
