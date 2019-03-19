using System;
using System.Linq.Expressions;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Characteristic = GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Characteristic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Characteristic, CharacteristicViewModel>();
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