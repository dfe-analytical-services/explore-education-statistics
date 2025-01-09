#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPreReleaseSummaryService
{
    Task<Either<ActionResult, PreReleaseSummaryViewModel>> GetPreReleaseSummaryViewModel(
        Guid releaseVersionId,
        CancellationToken cancellationToken);
}
