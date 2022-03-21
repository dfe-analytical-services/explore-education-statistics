using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class MyPublicationPermissionsResolver : IMyPublicationPermissionsResolver
    {
        private readonly IUserService _userService;
 
        public MyPublicationPermissionsResolver(IUserService userService)
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
                CanUpdatePublication = _userService
                    .CheckCanUpdatePublication(source)
                    .Result
                    .IsRight,
                CanUpdatePublicationTitle = _userService
                    .CheckCanUpdatePublicationTitle()
                    .Result
                    .IsRight,
                CanCreateReleases = _userService
                    .CheckCanCreateReleaseForPublication(source)
                    .Result
                    .IsRight,
                CanAdoptMethodologies = _userService
                    .CheckCanAdoptMethodologyForPublication(source)
                    .Result
                    .IsRight,
                CanCreateMethodologies = _userService
                    .CheckCanCreateMethodologyForPublication(source)
                    .Result
                    .IsRight,
                CanManageExternalMethodology = _userService
                    .CheckCanManageExternalMethodologyForPublication(source)
                    .Result
                    .IsRight
            };
        }
    }
}
