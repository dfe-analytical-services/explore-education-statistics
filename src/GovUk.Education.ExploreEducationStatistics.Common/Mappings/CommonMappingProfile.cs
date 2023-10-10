using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Common.Mappings;

public class CommonMappingProfile: Profile
{
    public CommonMappingProfile()
    {
        CreateMap<Contact, ContactViewModel>();
    }
}