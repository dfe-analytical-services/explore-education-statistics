using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface ILegacyReleaseService
    {
        Task<Either<ActionResult, LegacyReleaseViewModel>> GetLegacyRelease(Guid id);

        Task<Either<ActionResult, LegacyReleaseViewModel>> CreateLegacyRelease(
            CreateLegacyReleaseViewModel legacyRelease
        );

        Task<Either<ActionResult, LegacyReleaseViewModel>> UpdateLegacyRelease(
            Guid id, 
            UpdateLegacyReleaseViewModel legacyRelease
        );

        Task<Either<ActionResult, bool>> DeleteLegacyRelease(Guid id);
    }
}