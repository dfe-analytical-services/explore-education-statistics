#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.MethodologyVersionSummaryViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;

public static class PermissionsUtils
{
    public static ReleasePermissions GetReleasePermissions(IUserService userService, Release release)
    {
        return new ReleasePermissions
        {
            CanAddPrereleaseUsers = CheckHasPermission(userService.CheckCanAssignPrereleaseContactsToRelease(release)),
            CanUpdateRelease = CheckHasPermission(userService.CheckCanUpdateRelease(release)),
            CanDeleteRelease = CheckHasPermission(userService.CheckCanDeleteRelease(release)),
            CanMakeAmendmentOfRelease = CheckHasPermission(userService.CheckCanMakeAmendmentOfRelease(release))
        };
    }

    public static async Task<MethodologyVersionPermissions> GetMethodologyVersionPermissions(
        IUserService userService,
        MethodologyVersion methodologyVersion,
        PublicationMethodology publicationMethodology)
    {
        return new MethodologyVersionPermissions
        {
            CanDeleteMethodology =
                await userService.CheckCanDeleteMethodologyVersion(methodologyVersion).IsRight(),
            CanUpdateMethodology =
                await userService.CheckCanUpdateMethodologyVersion(methodologyVersion).IsRight(),
            CanApproveMethodology =
                await userService.CheckCanApproveMethodologyVersion(methodologyVersion).IsRight(),
            CanMarkMethodologyAsDraft =
                await userService.CheckCanMarkMethodologyVersionAsDraft(methodologyVersion).IsRight(),
            CanMakeAmendmentOfMethodology =
                await userService.CheckCanMakeAmendmentOfMethodology(methodologyVersion).IsRight(),
            CanRemoveMethodologyLink =
                await userService.CheckCanDropMethodologyLink(publicationMethodology).IsRight()
        };
    }

    private static bool CheckHasPermission(Task<Either<ActionResult, Release>> result)
    {
        return result.Result.IsRight;
    }
}
