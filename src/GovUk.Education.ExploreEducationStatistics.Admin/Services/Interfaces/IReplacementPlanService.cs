#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReplacementPlanService
    {
        Task<Either<ActionResult, DataReplacementPlanViewModel>> GetReplacementPlan(
            Guid releaseVersionId,
            Guid originalFileId,
            CancellationToken cancellationToken = default);

        Task<Either<ActionResult, DataReplacementPlanViewModel>> GenerateReplacementPlan(
            ReleaseFile originalReleaseFile,
            ReleaseFile replacementReleaseFile,
            CancellationToken cancellationToken = default);
    }
}
