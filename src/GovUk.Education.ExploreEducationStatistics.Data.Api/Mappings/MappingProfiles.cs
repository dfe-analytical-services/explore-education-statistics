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
            CreateMap<Characteristic, CharacteristicViewModel>()
                .ForMember(destinationMember => destinationMember.Name,
                    opts => opts.MapFrom(characteristic => characteristic.Breakdown));
            CreateMap<CharacteristicMeta, CharacteristicMetaViewModel>();
            CreateMap<IndicatorMeta, IndicatorMetaViewModel>()
                .ForMember(dest => dest.Unit, opts => opts.MapFrom(MapIndicatorMetaUnitExpression()));
            CreateMap<Subject, SubjectMetaViewModel>()
                .ForMember(dest => dest.Label, opts => opts.MapFrom(subject => subject.Name));
        }

        private static Expression<Func<IndicatorMeta, string>> MapIndicatorMetaUnitExpression()
        {
            return indicatorMeta => indicatorMeta.Unit == Unit.Percent ? "%" : "";
        }
    }
}