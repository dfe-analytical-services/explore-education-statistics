using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class MyPublicationPermissionSetPropertyResolver : IMyPublicationPermissionSetPropertyResolver
    {
        private readonly IUserService _userService;
 
        public MyPublicationPermissionSetPropertyResolver(IUserService userService)
        {
            _userService = userService;
        }
 
        public MyPublicationViewModel.PermissionsSet Resolve(
            Publication source, 
            MyPublicationViewModel destination, 
            MyPublicationViewModel.PermissionsSet destMember, 
            ResolutionContext context)
        {
            return new MyPublicationViewModel.PermissionsSet
            {
                CanCreateReleases =  _userService
                    .CheckCanCreateReleaseForPublication(source)
                    .Result
                    .OnSuccess(_ => true)
                    .OrElse(() => false)
                    .Right
            };
        }
    }
}