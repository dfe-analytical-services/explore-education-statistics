using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface ICacheKeyService
    {
        Task<Either<ActionResult, FastTrackResultsCacheKey>> CreateCacheKeyForFastTrackResults(Guid fastTrackId);
        Task<Either<ActionResult, SubjectMetaCacheKey>> CreateCacheKeyForSubjectMeta(Guid subjectId);
    }
}