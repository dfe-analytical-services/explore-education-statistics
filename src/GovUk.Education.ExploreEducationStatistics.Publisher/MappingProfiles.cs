using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Link, LinkViewModel>();

            CreateMap<Contact, ContactViewModel>();

            CreateContentBlockMap();

            CreateMap<ContentSection, ContentSectionViewModel>().ForMember(dest => dest.Content,
                m => m.MapFrom(section => section.Content.OrderBy(contentBlock => contentBlock.Order)));

            //CreateMap<ExternalMethodology, ExternalMethodologyViewModel>(); // @MarkFix remove map?

            //CreateMap<LegacyRelease, LegacyReleaseViewModel>(); // @MarkFix remove map?

            //CreateMap<Publication, PublicationTitleViewModel>();  // @MarkFix remove map?

            //CreateMap<Publication, CachedPublicationViewModel>()  // @MarkFix remove map?
            //    .ForMember(dest => dest.LegacyReleases,
            //        m => m.MapFrom(p => p.LegacyReleases.OrderByDescending(l => l.Order)))
            //    .ForMember(dest => dest.Releases, m => m.Ignore());

            CreateMap<Release, CachedReleaseViewModel>()
                .ForMember(dest => dest.CoverageTitle,
                    m => m.MapFrom(release => release.TimePeriodCoverage.GetEnumLabel()))
                .ForMember(dest => dest.Type,
                    m => m.MapFrom(release => new ReleaseTypeViewModel
                    {
                        Title = release.Type.GetTitle()
                    }))
                .ForMember(
                    dest => dest.Updates,
                    m => m.MapFrom(r => r.Updates.OrderByDescending(update => update.On)))
                .ForMember(
                    dest => dest.Content,
                    m => m.MapFrom(r => r.GenericContent.OrderBy(s => s.Order)));

            //CreateMap<Release, ReleaseTitleViewModel>();  // @MarkFix remove map?

            //CreateMap<Theme, ThemeViewModel>();  // @MarkFix remove map?

            //CreateMap<Topic, TopicViewModel>();  // @MarkFix remove map?

            CreateMap<Update, ReleaseNoteViewModel>();
        }

        private void CreateContentBlockMap()
        {
            CreateMap<ContentBlock, IContentBlockViewModel>()
                .IncludeAllDerived();

            CreateDataBlockMap();

            CreateMap<HtmlBlock, HtmlBlockViewModel>();

            CreateMap<MarkDownBlock, MarkDownBlockViewModel>();
        }

        private void CreateDataBlockMap()
        {
            CreateMap<DataBlock, DataBlockViewModel>();

            CreateMap<DataBlockSummary, DataBlockSummaryViewModel>();
        }
    }
}
