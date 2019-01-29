using System;
using System.Linq.Expressions;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
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
            CreateMap<GeographicModel, GeographicData>();
            CreateMap<CharacteristicViewModel, Characteristic>();
            CreateMap<CharacteristicMeta, NameLabelViewModel>();
            CreateMap<AttributeMeta, AttributeMetaViewModel>()
                .ForMember(dest => dest.Unit,
                    opts => opts.MapFrom(MapAttributeMetaUnitExpression()));
        }

        private static Expression<Func<AttributeMeta, string>> MapAttributeMetaUnitExpression()
        {
            return attributeMeta => attributeMeta.Unit == Unit.Percent ? "%" : "";
        }
    }
}