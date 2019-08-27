using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class DataMappingProfiles : Profile
    {
        public DataMappingProfiles()
        {
            CreateMap<IObservationalUnit, LabelValue>()
                .ForMember(dest => dest.Label, opts => { opts.MapFrom(unit => unit.Name); })
                .ForMember(dest => dest.Value, opts => { opts.MapFrom(unit => unit.Code); });

            CreateMap<Indicator, IndicatorMetaViewModel>()
                .ForMember(dest => dest.Value, opts => opts.MapFrom(indicator => indicator.Id))
                .ForMember(dest => dest.Unit, opts => opts.MapFrom(indicator => indicator.Unit.GetEnumValue()));

            CreateMap<Location, LocationViewModel>();

            CreateMap<Publication, PublicationMetaViewModel>();

            CreateMap<Subject, IdLabel>()
                .ForMember(dest => dest.Label, opts => opts.MapFrom(subject => subject.Name));

            CreateMap<Theme, ThemeMetaViewModel>();

            CreateMap<Topic, TopicMetaViewModel>();
        }
    }
}