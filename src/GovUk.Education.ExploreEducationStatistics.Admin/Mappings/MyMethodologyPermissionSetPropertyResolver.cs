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
            Methodology methodology,
            MyMethodologyViewModel destination,
            PermissionsSet destMember,
            ResolutionContext context)
        {
            return new PermissionsSet
            {
                CanApproveMethodology = CheckResult(_userService.CheckCanApproveMethodology(methodology)),
                CanUpdateMethodology = CheckResult(_userService.CheckCanUpdateMethodology(methodology)),
                CanDeleteMethodology = CheckResult(_userService.CheckCanDeleteMethodology(methodology)),
                CanMakeAmendmentOfMethodology =
                    CheckResult(_userService.CheckCanMakeAmendmentOfMethodology(methodology)),
                CanMarkMethodologyAsDraft = CheckResult(_userService.CheckCanMarkMethodologyAsDraft(methodology))
            };
        }

        private static bool CheckResult(Task<Either<ActionResult, Methodology>> result)
        {
            return result.Result.IsRight;
        }
    }
}
