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
            CreateMap<CharacteristicMeta, NameLabelViewModel>();
            CreateMap<IndicatorMeta, IndicatorMetaViewModel>()
                .ForMember(dest => dest.Unit,
                    opts => opts.MapFrom(MapIndicatorMetaUnitExpression()));
        }

        private static Expression<Func<IndicatorMeta, string>> MapIndicatorMetaUnitExpression()
        {
            return indicatorMeta => indicatorMeta.Unit == Unit.Percent ? "%" : "";
        }
    }
}