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

            CreateMap<ExternalMethodology, ExternalMethodologyViewModel>();

            CreateMap<LegacyRelease, LegacyReleaseViewModel>();

            CreateMap<Publication, PublicationTitleViewModel>();

            CreateMap<Publication, CachedPublicationViewModel>()
                .ForMember(dest => dest.LegacyReleases,
                    m => m.MapFrom(p => p.LegacyReleases.OrderByDescending(l => l.Order)))
                .ForMember(dest => dest.Releases, m => m.Ignore());

            CreateMap<Release, CachedReleaseViewModel>()
                .ForMember(dest => dest.CoverageTitle,
                    m => m.MapFrom(release => release.TimePeriodCoverage.GetEnumLabel()))
                .ForMember(
                    dest => dest.Updates,
                    m => m.MapFrom(r => r.Updates.OrderByDescending(update => update.On)))
                .ForMember(
                    dest => dest.Content,
                    m => m.MapFrom(r => r.GenericContent.OrderBy(s => s.Order)));

            CreateMap<Release, ReleaseTitleViewModel>();

            CreateMap<ReleaseType, ReleaseTypeViewModel>();

            CreateMap<Theme, ThemeViewModel>();

            CreateMap<Topic, TopicViewModel>();

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
