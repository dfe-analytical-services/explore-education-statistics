#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.MyPublicationMethodologyVersionViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class MyPublicationMethodologyVersionPermissionsResolver
        : IMyPublicationMethodologyVersionPermissionsResolver
    {
        private readonly IUserService _userService;

        public MyPublicationMethodologyVersionPermissionsResolver(IUserService userService)
        {
            _userService = userService;
        }

        public PermissionsSet Resolve(
            PublicationMethodology source,
            MyPublicationMethodologyVersionViewModel destination,
            PermissionsSet destMember,
            ResolutionContext context)
        {
            return new PermissionsSet
            {
                CanDropMethodology = _userService
                    .CheckCanDropMethodologyLink(source)
                    .Result
                    .IsRight
            };
        }
    }
}
