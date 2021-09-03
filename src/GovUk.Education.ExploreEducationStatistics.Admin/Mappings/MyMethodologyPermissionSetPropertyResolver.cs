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
using static GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.MyMethodologyViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class MyMethodologyPermissionSetPropertyResolver : IMyMethodologyPermissionSetPropertyResolver
    {
        private readonly IUserService _userService;

        public MyMethodologyPermissionSetPropertyResolver(IUserService userService)
        {
            _userService = userService;
        }

        public PermissionsSet Resolve(
            MethodologyVersion methodologyVersion,
            MyMethodologyViewModel destination,
            PermissionsSet destMember,
            ResolutionContext context)
        {
            return new PermissionsSet
            {
                CanApproveMethodology = CheckResult(_userService.CheckCanApproveMethodology(methodologyVersion)),
                CanUpdateMethodology = CheckResult(_userService.CheckCanUpdateMethodology(methodologyVersion)),
                CanDeleteMethodology = CheckResult(_userService.CheckCanDeleteMethodology(methodologyVersion)),
                CanMakeAmendmentOfMethodology =
                    CheckResult(_userService.CheckCanMakeAmendmentOfMethodology(methodologyVersion)),
                CanMarkMethodologyAsDraft = CheckResult(_userService.CheckCanMarkMethodologyAsDraft(methodologyVersion))
            };
        }

        private static bool CheckResult(Task<Either<ActionResult, MethodologyVersion>> result)
        {
            return result.Result.IsRight;
        }
    }
}
