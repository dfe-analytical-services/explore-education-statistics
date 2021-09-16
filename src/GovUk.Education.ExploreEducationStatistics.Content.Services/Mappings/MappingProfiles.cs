#nullable enable
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateContentBlockMap();

            CreateMap<ContentSection, ContentSectionViewModel>()
                .ForMember(dest => dest.Content,
                    m => m.MapFrom(section =>
                        section.Content.OrderBy(contentBlock => contentBlock.Order)));

            CreateMap<MethodologyNote, MethodologyNoteViewModel>();

            CreateMap<MethodologyVersion, MethodologySummaryViewModel>();

            CreateMap<MethodologyVersion, MethodologyViewModel>()
                .ForMember(dest => dest.Annexes,
                    m => m.MapFrom(methodologyVersion =>
                        methodologyVersion.Annexes.OrderBy(annexSection => annexSection.Order)))
                .ForMember(dest => dest.Content,
                    m => m.MapFrom(methodologyVersion =>
                        methodologyVersion.Content.OrderBy(contentSection => contentSection.Order)))
                .ForMember(dest => dest.Notes,
                    m => m.MapFrom(methodologyVersion =>
                        methodologyVersion.Notes.OrderByDescending(note => note.DisplayDate)));
        }

        private void CreateContentBlockMap()
        {
            CreateMap<ContentBlock, IContentBlockViewModel>()
                .IncludeAllDerived();

            CreateMap<HtmlBlock, HtmlBlockViewModel>();
        }
    }
}
