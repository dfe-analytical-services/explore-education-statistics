using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class MyMethodologyPermissionSetPropertyResolver : IMyMethodologyPermissionSetPropertyResolver
    {
        private readonly IUserService _userService;
 
        public MyMethodologyPermissionSetPropertyResolver(IUserService userService)
        {
            _userService = userService;
        }
 
        public MyMethodologyViewModel.PermissionsSet Resolve(
            Methodology methodology, 
            MyMethodologyViewModel destination, 
            MyMethodologyViewModel.PermissionsSet destMember, 
            ResolutionContext context)
        {
            return new MyMethodologyViewModel.PermissionsSet
            {
                CanUpdateMethodology = CheckResult(_userService.CheckCanUpdateMethodology(methodology)),
                CanDeleteMethodology = CheckResult(_userService.CheckCanDeleteMethodology(methodology)),
                CanMakeAmendmentOfMethodology = CheckResult(_userService.CheckCanMakeAmendmentOfMethodology(methodology))
            };
        }

        private static bool CheckResult(Task<Either<ActionResult, Methodology>> result)
        {
            return result.Result.IsRight;
        }
    }
}