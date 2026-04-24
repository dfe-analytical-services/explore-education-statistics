using GovUk.Education.ExploreEducationStatistics.Common.Mappings;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Mappings;

/**
 * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
 */
public class MappingProfiles : CommonMappingProfile
{
    public MappingProfiles()
    {
        CreateMap<MethodologyVersion, MethodologyVersionSummaryViewModel>();
        MapTypesForMethodologyVersionViewModel();
    }

    private void MapTypesForMethodologyVersionViewModel()
    {
        // The following configs are all required to map MethodologyVersion to MethodologyVersionViewModel

        CreateMap<ContentSection, ContentSectionViewModel>()
            .ForMember(
                dest => dest.Content,
                m => m.MapFrom(section => section.Content.OrderBy(contentBlock => contentBlock.Order))
            );

        CreateMap<MethodologyNote, MethodologyNoteViewModel>();

        CreateMap<MethodologyVersion, MethodologyVersionViewModel>()
            .ForMember(
                dest => dest.Annexes,
                m =>
                    m.MapFrom(methodologyVersion =>
                        methodologyVersion.MethodologyContent.Annexes.OrderBy(annexSection => annexSection.Order)
                    )
            )
            .ForMember(
                dest => dest.Content,
                m =>
                    m.MapFrom(methodologyVersion =>
                        methodologyVersion.MethodologyContent.Content.OrderBy(contentSection => contentSection.Order)
                    )
            )
            .ForMember(
                dest => dest.Notes,
                m =>
                    m.MapFrom(methodologyVersion =>
                        methodologyVersion.Notes.OrderByDescending(note => note.DisplayDate)
                    )
            );

        CreateMap<ContentBlock, IContentBlockViewModel>().IncludeAllDerived();

        CreateMap<HtmlBlock, HtmlBlockViewModel>();
    }
}
