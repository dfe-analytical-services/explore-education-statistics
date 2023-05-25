#nullable enable
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

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

            CreateMap<MethodologyVersion, MethodologyVersionSummaryViewModel>();

            CreateMap<MethodologyVersion, MethodologyVersionViewModel>()
                .ForMember(dest => dest.Annexes,
                    m => m.MapFrom(methodologyVersion =>
                        methodologyVersion.MethodologyContent.Annexes.OrderBy(annexSection => annexSection.Order)))
                .ForMember(dest => dest.Content,
                    m => m.MapFrom(methodologyVersion =>
                        methodologyVersion.MethodologyContent.Content.OrderBy(contentSection => contentSection.Order)))
                .ForMember(dest => dest.Notes,
                    m => m.MapFrom(methodologyVersion =>
                        methodologyVersion.Notes.OrderByDescending(note => note.DisplayDate)));

            CreateMap<Publication, PublicationSummaryViewModel>();

            CreateMap<KeyStatisticDataBlock, KeyStatisticDataBlockViewModel>();
            CreateMap<KeyStatisticText, KeyStatisticTextViewModel>();
            CreateMap<KeyStatistic, KeyStatisticViewModel>()
                .IncludeAllDerived();

            CreateMap<Release, ReleaseCacheViewModel>()
                .ForMember(dest => dest.CoverageTitle,
                    m => m.MapFrom(release => release.TimePeriodCoverage.GetEnumLabel()))
                .ForMember(
                    dest => dest.Updates,
                    m => m.MapFrom(r => r.Updates.OrderByDescending(update => update.On)))
                .ForMember(
                    dest => dest.Content,
                    m => m.MapFrom(r => r.GenericContent.OrderBy(s => s.Order)))
                .ForMember(
                    dest => dest.KeyStatistics,
                    m => m.MapFrom(r => r.KeyStatistics.OrderBy(ks => ks.Order)));

            CreateMap<Link, LinkViewModel>();

            CreateMap<Update, ReleaseNoteViewModel>();
        }

        private void CreateContentBlockMap()
        {
            CreateMap<ContentBlock, IContentBlockViewModel>()
                .IncludeAllDerived();

            CreateMap<DataBlock, DataBlockViewModel>();

            CreateMap<EmbedBlockLink, EmbedBlockLinkViewModel>()
                .ForMember(dest => dest.Title,
                    m => m.MapFrom(embedBlockLink =>
                        embedBlockLink.EmbedBlock.Title))
                .ForMember(dest => dest.Url,
                    m => m.MapFrom(embedBlockLink =>
                        embedBlockLink.EmbedBlock.Url));

            CreateMap<HtmlBlock, HtmlBlockViewModel>();

            CreateMap<MarkDownBlock, MarkDownBlockViewModel>();
        }
    }
}
