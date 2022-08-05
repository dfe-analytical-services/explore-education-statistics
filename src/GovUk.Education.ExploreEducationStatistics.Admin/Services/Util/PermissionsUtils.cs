using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;

public static class PermissionsUtils
{
   public static ReleaseViewModel.PermissionsSet GetPermissionsSet(IUserService userService, Release release)
   {
       return new ReleaseViewModel.PermissionsSet
       {
           CanAddPrereleaseUsers = CheckHasPermission(userService.CheckCanAssignPrereleaseContactsToRelease(release)),
           CanUpdateRelease = CheckHasPermission(userService.CheckCanUpdateRelease(release)),
           CanDeleteRelease = CheckHasPermission(userService.CheckCanDeleteRelease(release)),
           CanMakeAmendmentOfRelease = CheckHasPermission(userService.CheckCanMakeAmendmentOfRelease(release))
       };
   }

   private static bool CheckHasPermission(Task<Either<ActionResult, Release>> result)
   {
       return result
           .Result
           .OnSuccess(_ => true)
           .OrElse(() => false)
           .Right;
   }
}
