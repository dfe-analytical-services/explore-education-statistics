using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class MyReleasePermissionSetPropertyResolver : IMyReleasePermissionSetPropertyResolver
    {
        private readonly IUserService _userService;
 
        public MyReleasePermissionSetPropertyResolver(IUserService userService)
        {
            _userService = userService;
        }
 
        public MyReleaseViewModel.PermissionsSet Resolve(
            Release release, 
            MyReleaseViewModel destination, 
            MyReleaseViewModel.PermissionsSet destMember, 
            ResolutionContext context)
        {
            return new MyReleaseViewModel.PermissionsSet
            {
                CanUpdateRelease = CheckResult(_userService.CheckCanUpdateRelease(release)),
                CanDeleteRelease = CheckResult(_userService.CheckCanDeleteRelease(release)),
                CanMakeAmendmentOfRelease = CheckResult(_userService.CheckCanMakeAmendmentOfRelease(release))
            };
        }

        private static bool CheckResult(Task<Either<ActionResult, Release>> result)
        {
            return result
                .Result
                .OnSuccess(_ => true)
                .OrElse(() => false)
                .Right;
        }
    }
}