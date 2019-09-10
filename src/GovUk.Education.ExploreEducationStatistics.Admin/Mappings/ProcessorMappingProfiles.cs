using AutoMapper;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class ProcessorMappingProfiles : Profile
    {
        public ProcessorMappingProfiles()
        {
            CreateMap<Content.Model.Release, Data.Processor.Model.Release>()
                .ForMember(dest => dest.ReleaseDate, opts => { opts.MapFrom(release => release.PublishScheduled); });
            CreateMap<Content.Model.Publication, Data.Processor.Model.Publication>();
            CreateMap<Content.Model.Topic, Data.Processor.Model.Topic>();
            CreateMap<Content.Model.Theme, Data.Processor.Model.Theme>();
        }
    }
}