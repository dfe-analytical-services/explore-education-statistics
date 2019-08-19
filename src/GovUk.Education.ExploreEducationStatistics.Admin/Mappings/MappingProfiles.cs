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
                .ForMember(dest => dest.Contact,
                    m => m.MapFrom(r => r.Publication.Contact))
                .ForMember(dest => dest.Published,
                    m => m.MapFrom(r => r.Published))
                .ForMember(dest => dest.Title,
                    m => m.MapFrom(r => r.ReleaseSummary.Title))
                .ForMember(dest => dest.CoverageTitle,
                    m => m.MapFrom(r => r.ReleaseSummary.CoverageTitle))
                .ForMember(dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id))
                .ForMember(dest => dest.PublicationTitle,
                    m => m.MapFrom(r => r.Publication.Title))
                .ForMember(dest => dest.PublishScheduled,
                    m => m.MapFrom(r => r.ReleaseSummary.PublishScheduled))
                .ForMember(dest => dest.ReleaseName,
                    m => m.MapFrom(r => r.ReleaseSummary.ReleaseName))
                .ForMember(dest => dest.TypeId,
                    m => m.MapFrom(r => r.ReleaseSummary.TypeId))
                .ForMember(dest => dest.Type,
                    m => m.MapFrom(r => r.ReleaseSummary.Type))
                .ForMember(dest => dest.YearTitle,
                    m => m.MapFrom(r => r.ReleaseSummary.YearTitle))
                .ForMember(dest => dest.NextReleaseDate,
                    m => m.MapFrom(r => r.ReleaseSummary.NextReleaseDate))
                .ForMember(dest => dest.TimePeriodCoverage,
                    m => m.MapFrom(r => r.ReleaseSummary.TimePeriodCoverage));
            
            CreateMap<CreateReleaseViewModel, ReleaseSummaryVersion>();
            CreateMap<EditReleaseSummaryViewModel, ReleaseSummaryVersion>();
            CreateMap<ReleaseSummary, EditReleaseSummaryViewModel>();
        }
    }
}