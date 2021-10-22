using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IFastTrackService
    {
        Task<Either<ActionResult, FastTrackViewModel>> GetFastTrackAndResults(Guid fastTrackId);

        Task<Either<ActionResult, ReleaseFastTrack>> GetReleaseFastTrack(Guid fastTrackId);
    }
}