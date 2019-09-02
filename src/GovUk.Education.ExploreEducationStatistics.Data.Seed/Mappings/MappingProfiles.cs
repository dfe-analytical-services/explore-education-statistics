using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Release, Processor.Model.Release>();
            CreateMap<Publication, Processor.Model.Publication>();
            CreateMap<Topic, Processor.Model.Topic>();
            CreateMap<Theme, Processor.Model.Theme>();
        }
    }
}