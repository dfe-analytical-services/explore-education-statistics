#nullable enable
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.MyMethodologyVersionViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class MyMethodologyVersionPermissionsResolver : IMyMethodologyVersionPermissionsResolver
    {
        private readonly IUserService _userService;

        public MyMethodologyVersionPermissionsResolver(IUserService userService)
        {
            _userService = userService;
        }

        public PermissionsSet Resolve(
            MethodologyVersion source,
            MyMethodologyVersionViewModel destination,
            PermissionsSet destMember,
            ResolutionContext context)
        {
            return new PermissionsSet
            {
                CanApproveMethodology = CheckResult(_userService.CheckCanApproveMethodology(source)),
                CanUpdateMethodology = CheckResult(_userService.CheckCanUpdateMethodology(source)),
                CanDeleteMethodology = CheckResult(_userService.CheckCanDeleteMethodology(source)),
                CanMakeAmendmentOfMethodology =
                    CheckResult(_userService.CheckCanMakeAmendmentOfMethodology(source)),
                CanMarkMethodologyAsDraft = CheckResult(_userService.CheckCanMarkMethodologyAsDraft(source))
            };
        }

        private static bool CheckResult(Task<Either<ActionResult, MethodologyVersion>> result)
        {
            return result.Result.IsRight;
        }
    }
}
