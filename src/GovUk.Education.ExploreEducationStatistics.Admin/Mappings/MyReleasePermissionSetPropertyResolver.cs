using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class MyReleasePermissionSetPropertyResolver
        : IValueResolver<Release, MyReleaseViewModel, MyReleaseViewModel.PermissionsSet>
    {
        private readonly IUserService _userService;
 
        public MyReleasePermissionSetPropertyResolver(IUserService userService)
        {
            _userService = userService;
        }
 
        public MyReleaseViewModel.PermissionsSet Resolve(
            Release source, 
            MyReleaseViewModel destination, 
            MyReleaseViewModel.PermissionsSet destMember, 
            ResolutionContext context)
        {
            return new MyReleaseViewModel.PermissionsSet
            {
                CanUpdateRelease = _userService
                    .CheckCanUpdateRelease(source)
                    .Result
                    .OnSuccess(_ => true)
                    .OrElse(() => false)
                    .Right
            };
        }
    }
}