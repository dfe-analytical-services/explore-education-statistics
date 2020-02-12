using System.Linq;
using AutoMapper;
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
            CreateMap<BasicLink, LinkViewModel>();

            CreateMap<Contact, ContactViewModel>();

            CreateContentBlockMap();

            CreateMap<ContentSection, ContentSectionViewModel>().ForMember(dest => dest.Content,
                m => m.MapFrom(section => section.Content.OrderBy(contentBlock => contentBlock.Order)));

            CreateMap<ExternalMethodology, ExternalMethodologyViewModel>();

            CreateMap<Link, LinkViewModel>();

            CreateMap<Methodology, MethodologySummaryViewModel>();

            CreateMap<Methodology, MethodologyViewModel>();

            CreateMap<Publication, PublicationTitleViewModel>();

            CreateMap<Publication, CachedPublicationViewModel>()
                .ForMember(dest => dest.LegacyReleases,
                    m => m.MapFrom(p => p.LegacyReleases.OrderBy(l => l.Description)))
                .ForMember(dest => dest.Releases, m => m.Ignore());

            CreateMap<Release, CachedReleaseViewModel>()
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
            CreateMap<IContentBlock, IContentBlockViewModel>()
                .IncludeAllDerived();

            CreateDataBlockMap();

            CreateMap<HtmlBlock, HtmlBlockViewModel>();

            CreateMap<InsetTextBlock, InsetTextBlockViewModel>();

            CreateMap<MarkDownBlock, MarkDownBlockViewModel>();
        }

        private void CreateDataBlockMap()
        {
            CreateMap<DataBlock, DataBlockViewModel>();

            CreateMap<Summary, DataBlockSummaryViewModel>();

            CreateMap<Table, DataBlockTableViewModel>();

            CreateMap<TableHeaders, DataBlockTableHeadersViewModel>();

            CreateMap<TableOption, DataBlockTableOptionViewModel>();

            CreateMap<TableRowGroupOption, DataBlockTableRowGroupOptionViewModel>();
        }

        private static ContentSectionViewModel ContentSectionToViewModel(ContentSection section)
        {
            return new ContentSectionViewModel
            {
                Id = section.Id,
                Caption = section.Caption,
                //Content = section.Content?.OrderBy(contentBlock => contentBlock.Order).ToList(),
                Heading = section.Heading,
                Order = section.Order
            };
        }
    }
}