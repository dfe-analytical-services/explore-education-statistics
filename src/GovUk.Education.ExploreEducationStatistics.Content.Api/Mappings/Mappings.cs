using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Release, PublicationViewModel>()
                .ForMember(dest => dest.Theme, opts => { opts.MapFrom(unit => unit.Publication.Topic.Theme.Title); });
        }
    }
}
