#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.MyPublicationMethodologyViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class MyPublicationMethodologyPermissionsPropertyResolver
        : IMyPublicationMethodologyPermissionsPropertyResolver
    {
        private readonly IUserService _userService;

        public MyPublicationMethodologyPermissionsPropertyResolver(IUserService userService)
        {
            _userService = userService;
        }

        public PermissionsSet Resolve(
            PublicationMethodology source,
            MyPublicationMethodologyViewModel destination,
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
