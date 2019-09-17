using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class ProcessorMappingProfiles : Profile
    {
        public ProcessorMappingProfiles()
        {
            CreateMap<Release, Data.Processor.Model.Release>()
                .ForMember(dest => dest.ReleaseDate, opts => { opts.MapFrom(release => release.PublishScheduled); });
            CreateMap<Publication, Data.Processor.Model.Publication>();
            CreateMap<Topic, Data.Processor.Model.Topic>();
            CreateMap<Theme, Data.Processor.Model.Theme>();
        }
    }
}