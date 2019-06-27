using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Release, Data.Processor.Model.Release>().ForMember(dest => dest.Title,
                opts => opts.MapFrom(release => release.ReleaseName));
        }
    }
}