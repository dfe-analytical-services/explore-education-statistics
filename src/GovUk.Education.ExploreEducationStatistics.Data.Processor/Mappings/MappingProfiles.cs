using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Release, Data.Model.Release>();
            CreateMap<Publication, Data.Model.Publication>();
            CreateMap<Topic, Data.Model.Topic>();
            CreateMap<Theme, Data.Model.Theme>();
        }
    }
}