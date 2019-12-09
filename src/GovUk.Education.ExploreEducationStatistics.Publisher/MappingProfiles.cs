using System;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<QueueReleaseMessage, ReleaseInfo>()
                .ForMember(dest => dest.Created, m => m.UseDestinationValue());
            
            CreateMap<Release, ReleaseViewModel>()
                .ForMember(
                    dest => dest.Content,
                    m => m.MapFrom(r => r.GenericContent));

            CreateMap<Theme, ThemeTree>()
                .ForMember(dest => dest.Topics, m => m.MapFrom(theme => theme.Topics));
        }
    }
}