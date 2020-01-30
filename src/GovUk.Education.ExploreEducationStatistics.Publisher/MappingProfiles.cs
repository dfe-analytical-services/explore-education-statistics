using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Contact, ContactViewModel>();

            CreateMap<ContentSection, ContentSectionViewModel>();

            CreateMap<Link, BasicLink>();

            CreateMap<Methodology, MethodologyViewModel>();

            CreateMap<Publication, PublicationTitleViewModel>();

            CreateMap<Publication, PublicationViewModel>()
                .ForMember(dest => dest.Releases, m => m.Ignore());

            CreateMap<Release, PreviousReleaseViewModel>();

            CreateMap<Release, ReleaseViewModel>()
                .ForMember(
                    dest => dest.Content,
                    m => m.MapFrom(r => r.GenericContent));

            CreateMap<Theme, ThemeViewModel>();

            CreateMap<Topic, TopicViewModel>();

            CreateMap<Update, ReleaseNoteViewModel>();
        }
    }
}