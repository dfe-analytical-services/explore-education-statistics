using System;
using System.Linq.Expressions;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Country, LabelValueViewModel>()
                .ForMember(dest => dest.Label, opts => { opts.MapFrom(country => country.Name); })
                .ForMember(dest => dest.Value, opts => { opts.MapFrom(country => country.Code); });

            CreateMap<Indicator, IndicatorMetaViewModel>()
                .ForMember(dest => dest.Value, opts => opts.MapFrom(indicator => indicator.Id))
                .ForMember(dest => dest.Unit, opts => opts.MapFrom(MapIndicatorUnitExpression()));

            CreateMap<LocalAuthority, LabelValueViewModel>()
                .ForMember(dest => dest.Label, opts => { opts.MapFrom(localAuthority => localAuthority.Name); })
                .ForMember(dest => dest.Value, opts => { opts.MapFrom(localAuthority => localAuthority.Code); });

            CreateMap<Region, LabelValueViewModel>()
                .ForMember(dest => dest.Label, opts => { opts.MapFrom(region => region.Name); })
                .ForMember(dest => dest.Value, opts => { opts.MapFrom(region => region.Code); });

            CreateMap<Subject, IdLabelViewModel>()
                .ForMember(dest => dest.Label, opts => opts.MapFrom(subject => subject.Name));

            CreateMap<Location, LocationViewModel>();
        }

        private static Expression<Func<Indicator, string>> MapIndicatorUnitExpression()
        {
            return indicatorMeta => indicatorMeta.Unit == Unit.Percent ? "%" : "";
        }
    }
}