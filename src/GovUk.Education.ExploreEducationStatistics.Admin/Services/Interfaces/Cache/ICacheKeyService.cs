#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache
{
    public interface ICacheKeyService
    {
        Task<Either<ActionResult, DataBlockTableResultCacheKey>> CreateCacheKeyForDataBlock(Guid releaseId,
            Guid dataBlockId);
    }
}