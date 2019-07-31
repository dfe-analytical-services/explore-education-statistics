using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public static class ModelMappers
    {
        public static readonly IMapper PublicationViewModelMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Publication, PublicationViewModel>();
            cfg.CreateMap<Release, ReleaseViewModel>()
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id));
            cfg.CreateMap<Methodology, MethodologyViewModel>();
        }).CreateMapper();


        public static readonly IMapper MethodologyViewModelMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Methodology, MethodologyViewModel>();
        }).CreateMapper();
    }
}