#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseChecklistService
{
    Task<Either<ActionResult, ReleaseChecklistViewModel>> GetChecklist(Guid releaseVersionId);

    Task<List<ReleaseChecklistIssue>> GetErrors(ReleaseVersion releaseVersion);

    Task<List<ReleaseChecklistIssue>> GetWarnings(ReleaseVersion releaseVersion);
}
