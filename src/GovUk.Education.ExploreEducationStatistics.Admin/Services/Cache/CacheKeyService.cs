#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Cache
{
    public class CacheKeyService : ICacheKeyService
    {
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;

        public CacheKeyService(IPersistenceHelper<ContentDbContext> contentPersistenceHelper)
        {
            _contentPersistenceHelper = contentPersistenceHelper;
        }
        
        public async Task<Either<ActionResult, DataBlockTableResultCacheKey>> CreateCacheKeyForDataBlock(Guid releaseId, Guid dataBlockId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<ReleaseContentBlock>(query => query
                    .Include(rcb => rcb.Release)
                    .Include(rcb => rcb.ContentBlock)
                    .Where(rcb => rcb.ContentBlockId == dataBlockId && rcb.ReleaseId == releaseId))
                .OnSuccess(releaseContentBlock => new DataBlockTableResultCacheKey(releaseContentBlock));
        }
    }
}