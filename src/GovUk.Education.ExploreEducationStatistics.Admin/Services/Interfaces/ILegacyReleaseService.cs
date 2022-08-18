#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface ILegacyReleaseService
    {
        Task<Either<ActionResult, LegacyReleaseViewModel>> GetLegacyRelease(Guid id);

        Task<Either<ActionResult, List<LegacyReleaseViewModel>>> ListLegacyReleases(Guid publicationId);

        Task<Either<ActionResult, LegacyReleaseViewModel>> CreateLegacyRelease(
            LegacyReleaseCreateViewModel legacyRelease
        );

        Task<Either<ActionResult, LegacyReleaseViewModel>> UpdateLegacyRelease(
            Guid id,
            LegacyReleaseUpdateViewModel legacyRelease
        );

        Task<Either<ActionResult, bool>> DeleteLegacyRelease(Guid id);
    }
}
