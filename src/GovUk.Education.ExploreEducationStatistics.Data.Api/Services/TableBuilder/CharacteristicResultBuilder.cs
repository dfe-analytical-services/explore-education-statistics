using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class CharacteristicResultBuilder : IResultBuilder<ICharacteristicData, TableBuilderCharacteristicData>
    {
        private readonly IMapper _mapper;

        public CharacteristicResultBuilder(IMapper mapper)
        {
            _mapper = mapper;
        }

        public TableBuilderCharacteristicData BuildResult(ICharacteristicData characteristicData,
            ICollection<string> attributeFilter)
        {
            return new TableBuilderCharacteristicData
            {
                Year = characteristicData.Year.ToString(),
                Characteristic = _mapper.Map<CharacteristicViewModel>(characteristicData.Characteristic),
                Range = attributeFilter.Count > 0
                    ? QueryUtil.FilterAttributes(characteristicData.Attributes, attributeFilter)
                    : characteristicData.Attributes
            };
        }
    }
}