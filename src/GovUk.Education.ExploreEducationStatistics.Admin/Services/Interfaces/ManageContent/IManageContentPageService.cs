#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;

public interface IManageContentPageService
{
    Task<Either<ActionResult, ManageContentPageViewModel>> GetManageContentPageViewModel(
        Guid releaseVersionId,
        bool isPrerelease = false);
}
