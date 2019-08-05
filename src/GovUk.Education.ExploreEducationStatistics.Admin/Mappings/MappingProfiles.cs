using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
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
            
            CreateMap<Release, ReleaseViewModel>()
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id))
                .ForMember(dest => dest.Contact, 
                    m => m.MapFrom(r => r.Publication.Contact))
                .ForMember(dest => dest.PublicationTitle, 
                    m => m.MapFrom(r => r.Publication.Title));;
            
            CreateMap<EditReleaseSummaryViewModel, Release>();
        }
    }
}