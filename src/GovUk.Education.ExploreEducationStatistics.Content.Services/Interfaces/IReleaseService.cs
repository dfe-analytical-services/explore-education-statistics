#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<Either<ActionResult, CachedReleaseViewModel>> GetRelease(Guid releaseId, DateTime published);

        Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> List(string publicationSlug);
    }
}
