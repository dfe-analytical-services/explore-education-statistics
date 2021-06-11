using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.Azure.Management.DataFactory.Models;

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

            CreateMap<PublicationMethodology, MethodologySummaryViewModel>()
                .IncludeMembers(pm => pm.MethodologyParent);

            CreateMap<MethodologyParent, MethodologySummaryViewModel>();

            CreateMap<Publication, PublicationTitleViewModel>();

            CreateMap<Publication, CachedPublicationViewModel>()
                .ForMember(dest => dest.LegacyReleases,
                    m => m.MapFrom(p => p.LegacyReleases.OrderByDescending(l => l.Order)))
                .ForMember(dest => dest.Methodologies,
                    m => m.MapFrom(p => p.Methodologies))
                .ForMember(dest => dest.Releases, m => m.Ignore());

            CreateMap<Publication, Data.Model.Publication>()
                .ForMember(dest => dest.Releases, m => m.Ignore())
                .ForMember(dest => dest.Topic, m => m.Ignore());

            CreateMap<Release, CachedReleaseViewModel>()
                .ForMember(dest => dest.CoverageTitle,
                    m => m.MapFrom(release => release.TimePeriodCoverage.GetEnumLabel()))
                .ForMember(
                    dest => dest.Content,
                    m => m.MapFrom(r => r.GenericContent.OrderBy(s => s.Order)));

            CreateMap<Release, ReleaseTitleViewModel>();

            CreateMap<ReleaseType, ReleaseTypeViewModel>();

            CreateMap<Theme, ThemeViewModel>();

            CreateMap<Theme, Data.Model.Theme>()
                .ForMember(dest => dest.Topics, m => m.Ignore());

            CreateMap<Topic, TopicViewModel>();

            CreateMap<Topic, Data.Model.Topic>()
                .ForMember(dest => dest.Publications, m => m.Ignore())
                .ForMember(dest => dest.Theme, m => m.Ignore());

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
