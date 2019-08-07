using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<IObservationalUnit, LabelValueViewModel>()
                .ForMember(dest => dest.Label, opts => { opts.MapFrom(unit => unit.Name); })
                .ForMember(dest => dest.Value, opts => { opts.MapFrom(unit => unit.Code); });

            CreateMap<Indicator, IndicatorMetaViewModel>()
                .ForMember(dest => dest.Value, opts => opts.MapFrom(indicator => indicator.Id))
                .ForMember(dest => dest.Unit, opts => opts.MapFrom(indicator => indicator.Unit.GetEnumValue()));

            CreateMap<Location, LocationViewModel>();

            CreateMap<Publication, PublicationMetaViewModel>();

            CreateMap<Subject, IdLabelViewModel>()
                .ForMember(dest => dest.Label, opts => opts.MapFrom(subject => subject.Name));

            CreateMap<Theme, ThemeMetaViewModel>();

            CreateMap<Topic, TopicMetaViewModel>();
        }
    }
}